using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigrationService.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace MigrationService.Controllers
{
    public class ApplicationsController : Controller
    {
        private readonly MigrationDbContext _context;

        public ApplicationsController(MigrationDbContext context)
        {
            _context = context;
        }

        // GET: Applications
        public async Task<IActionResult> Index(string searchString, string typeFilter, string[] statusFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["TypeFilter"] = typeFilter;
            ViewData["StatusFilters"] = statusFilter;

            // Build the SQL query manually to avoid the WITH clause issue
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("SELECT a.*, m.FullName AS MigrantName, o.FullName AS OfficerName ");
            sqlBuilder.Append("FROM Applications a ");
            sqlBuilder.Append("INNER JOIN Migrants m ON a.MigrantID = m.MigrantID ");
            sqlBuilder.Append("INNER JOIN Officers o ON a.OfficerID = o.OfficerID ");
            sqlBuilder.Append("WHERE 1=1 ");

            var parameters = new List<object>();

            // Add search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                sqlBuilder.Append("AND m.FullName LIKE @searchString ");
                parameters.Add(new SqlParameter("@searchString", $"%{searchString}%"));
            }

            // Add type filter
            if (!string.IsNullOrEmpty(typeFilter))
            {
                sqlBuilder.Append("AND a.Type = @typeFilter ");
                parameters.Add(new SqlParameter("@typeFilter", typeFilter));
            }

            // Add status filter(s)
            if (statusFilter != null && statusFilter.Length > 0)
            {
                sqlBuilder.Append("AND a.Status IN (");
                for (int i = 0; i < statusFilter.Length; i++)
                {
                    if (i > 0) sqlBuilder.Append(",");
                    sqlBuilder.Append($"@status{i} ");
                    parameters.Add(new SqlParameter($"@status{i}", statusFilter[i]));
                }
                sqlBuilder.Append(") ");
            }

            // Execute the raw SQL query
            var applications = new List<Application>();
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sqlBuilder.ToString();
                command.Parameters.AddRange(parameters.ToArray());

                if (command.Connection.State != System.Data.ConnectionState.Open)
                    await command.Connection.OpenAsync();

                using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {
                        var application = new Application
                        {
                            ApplicationID = result.GetInt32(result.GetOrdinal("ApplicationID")),
                            MigrantID = result.GetInt32(result.GetOrdinal("MigrantID")),
                            OfficerID = result.GetInt32(result.GetOrdinal("OfficerID")),
                            Type = result.GetString(result.GetOrdinal("Type")),
                            Status = result.GetString(result.GetOrdinal("Status")),
                            SubmissionDate = result.GetDateTime(result.GetOrdinal("SubmissionDate")),
                            DecisionDate = result.IsDBNull(result.GetOrdinal("DecisionDate")) ? null : (DateTime?)result.GetDateTime(result.GetOrdinal("DecisionDate"))
                        };

                        // Create related entities for display
                        application.Migrant = new Migrant { MigrantID = application.MigrantID, FullName = result.GetString(result.GetOrdinal("MigrantName")) };
                        application.Officer = new Officer { OfficerID = application.OfficerID, FullName = result.GetString(result.GetOrdinal("OfficerName")) };

                        applications.Add(application);
                    }
                }
            }

            return View(applications);
        }

        // GET: Applications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var application = await _context.Applications
                .Include(a => a.Migrant)
                .Include(a => a.Officer)
                .Include(a => a.Documents)
                .Include(a => a.StatusChanges)
                .FirstOrDefaultAsync(m => m.ApplicationID == id);

            if (application == null)
                return NotFound();

            return View(application);
        }

        // GET: Applications/Create
        public async Task<IActionResult> Create()
        {
            await PopulateViewBagData();
            return View();
        }

        // POST: Applications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MigrantID,OfficerID,Type,Status")] Application application)
        {
            // Log model state errors for debugging
            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Model Error: {error.ErrorMessage}");
                    }
                }
            }

            try
            {
                // Set default values
                application.SubmissionDate = DateTime.Now;

                // Add to database
                _context.Applications.Add(application);
                await _context.SaveChangesAsync();

                // Create initial status change
                var statusChange = new StatusChange
                {
                    ApplicationID = application.ApplicationID,
                    Status = application.Status,
                    ChangedAt = application.SubmissionDate,
                    Comment = "Initial application submission"
                };

                _context.StatusChanges.Add(statusChange);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the exception details
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                // Add a user-friendly error message
                ModelState.AddModelError("", $"Error creating application: {ex.Message}");
                
                // Repopulate the dropdown lists
                await PopulateViewBagData();
                return View(application);
            }
        }

        // GET: Applications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var application = await _context.Applications.FindAsync(id);
            if (application == null)
                return NotFound();
            
            await PopulateViewBagData();
            return View(application);
        }

        // POST: Applications/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ApplicationID,MigrantID,OfficerID,Type,Status,SubmissionDate,DecisionDate")] Application application, string statusComment)
        {
            // Log the incoming data
            System.Diagnostics.Debug.WriteLine($"Edit POST - ID: {id}, ApplicationID: {application.ApplicationID}");
            System.Diagnostics.Debug.WriteLine($"MigrantID: {application.MigrantID}, OfficerID: {application.OfficerID}");
            System.Diagnostics.Debug.WriteLine($"Type: {application.Type}, Status: {application.Status}");
            System.Diagnostics.Debug.WriteLine($"SubmissionDate: {application.SubmissionDate}, DecisionDate: {application.DecisionDate}");
            System.Diagnostics.Debug.WriteLine($"StatusComment: {statusComment}");

            if (id != application.ApplicationID)
            {
                System.Diagnostics.Debug.WriteLine("ID mismatch");
                return NotFound();
            }

            // Remove validation for navigation properties
            ModelState.Remove("Migrant");
            ModelState.Remove("Officer");
            ModelState.Remove("Documents");
            ModelState.Remove("StatusChanges");

            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("ModelState is invalid:");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Model Error: {error.ErrorMessage}");
                    }
                }
                await PopulateViewBagData();
                return View(application);
            }

            try
            {
                var existingApplication = await _context.Applications
                    .Include(a => a.StatusChanges)
                    .FirstOrDefaultAsync(a => a.ApplicationID == id);

                if (existingApplication == null)
                {
                    System.Diagnostics.Debug.WriteLine("Existing application not found");
                    return NotFound();
                }

                // Check if status has changed
                if (existingApplication.Status != application.Status)
                {
                    System.Diagnostics.Debug.WriteLine($"Status changed from {existingApplication.Status} to {application.Status}");
                    var statusChange = new StatusChange
                    {
                        ApplicationID = application.ApplicationID,
                        Status = application.Status,
                        ChangedAt = DateTime.Now,
                        Comment = string.IsNullOrEmpty(statusComment) ? $"Status changed from {existingApplication.Status} to {application.Status}" : statusComment
                    };
                    _context.StatusChanges.Add(statusChange);
                }

                // Update all fields
                existingApplication.MigrantID = application.MigrantID;
                existingApplication.OfficerID = application.OfficerID;
                existingApplication.Type = application.Type;
                existingApplication.Status = application.Status;
                existingApplication.SubmissionDate = application.SubmissionDate;
                existingApplication.DecisionDate = application.DecisionDate;

                System.Diagnostics.Debug.WriteLine("Saving changes to database");
                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("Changes saved successfully");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Concurrency exception: {ex.Message}");
                if (!ApplicationExists(application.ApplicationID))
                    return NotFound();
                else
                    throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                ModelState.AddModelError("", "An error occurred while saving the application.");
                await PopulateViewBagData();
                return View(application);
            }
        }

        private async Task PopulateViewBagData()
        {
            ViewData["MigrantID"] = new SelectList(await _context.Migrants.ToListAsync(), "MigrantID", "FullName");
            ViewData["OfficerID"] = new SelectList(await _context.Officers.ToListAsync(), "OfficerID", "FullName");
        }

        // GET: Applications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var application = await _context.Applications
                .Include(a => a.Migrant)
                .Include(a => a.Officer)
                .FirstOrDefaultAsync(m => m.ApplicationID == id);
            if (application == null)
                return NotFound();

            return View(application);
        }

        // POST: Applications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var application = await _context.Applications
                .Include(a => a.Documents)
                .Include(a => a.StatusChanges)
                .FirstOrDefaultAsync(a => a.ApplicationID == id);

            if (application == null)
                return NotFound();

            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ApplicationExists(int id)
        {
            return _context.Applications.Any(e => e.ApplicationID == id);
        }
    }
} 

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigrationService.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        public async Task<IActionResult> Index(string searchString, string typeFilter, string[] statusFilter, DateTime? startDate, DateTime? endDate, string sortBy)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["TypeFilter"] = typeFilter;
            ViewData["StatusFilters"] = statusFilter;
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["SortBy"] = sortBy;

            // Get all applications with related data
            var applications = await _context.Applications
                .Include(a => a.Migrant)
                .Include(a => a.Officer)
                .ToListAsync();

            // Apply filters in memory
            if (!string.IsNullOrEmpty(searchString))
            {
                applications = applications.Where(a => a.Migrant.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(typeFilter))
            {
                applications = applications.Where(a => a.Type == typeFilter).ToList();
            }

            if (statusFilter != null && statusFilter.Length > 0)
            {
                applications = applications.Where(a => statusFilter.Contains(a.Status)).ToList();
            }

            if (startDate.HasValue)
            {
                applications = applications.Where(a => a.SubmissionDate >= startDate.Value).ToList();
            }

            if (endDate.HasValue)
            {
                applications = applications.Where(a => a.SubmissionDate <= endDate.Value).ToList();
            }

            // Apply sorting
            applications = sortBy switch
            {
                "date_asc" => applications.OrderBy(a => a.SubmissionDate).ToList(),
                "name_asc" => applications.OrderBy(a => a.Migrant.FullName).ToList(),
                "name_desc" => applications.OrderByDescending(a => a.Migrant.FullName).ToList(),
                _ => applications.OrderByDescending(a => a.SubmissionDate).ToList() // Default to newest first
            };

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
            if (ModelState.IsValid)
            {
                application.SubmissionDate = DateTime.Now;

                _context.Add(application);
                await _context.SaveChangesAsync();

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
            await PopulateViewBagData();
            return View(application);
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
            if (id != application.ApplicationID)
                return NotFound();

            ModelState.Remove("statusComment");

            if (ModelState.IsValid)
            {
                var existingApplication = await _context.Applications
                    .Include(a => a.StatusChanges)
                    .FirstOrDefaultAsync(a => a.ApplicationID == id);

                if (existingApplication == null)
                    return NotFound();
        
                // Check if status has changed
                if (existingApplication.Status != application.Status)
                {
                    var statusChange = new StatusChange
                    {
                        ApplicationID = application.ApplicationID,
                        Status = application.Status,
                        ChangedAt = DateTime.Now,
                        Comment = !string.IsNullOrWhiteSpace(statusComment) 
                            ? statusComment 
                            : $"Статус изменен с {existingApplication.Status} на {application.Status}"
                    };
                    _context.StatusChanges.Add(statusChange);

                    // Set DecisionDate when status changes to Approved or Rejected
                    if (application.Status == "Approved" || application.Status == "Rejected")
                    {
                        application.DecisionDate = DateTime.Now;
                    }
                }

                // Update fields
                existingApplication.MigrantID = application.MigrantID;
                existingApplication.OfficerID = application.OfficerID;
                existingApplication.Type = application.Type;
                existingApplication.Status = application.Status;
                existingApplication.SubmissionDate = application.SubmissionDate;
                existingApplication.DecisionDate = application.DecisionDate;

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApplicationExists(application.ApplicationID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            await PopulateViewBagData();
            return View(application);
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

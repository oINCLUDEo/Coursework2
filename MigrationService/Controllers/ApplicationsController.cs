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
        public async Task<IActionResult> Index(string searchString, string typeFilter, string[] statusFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["TypeFilter"] = typeFilter;
            ViewData["StatusFilters"] = statusFilter;

            // Start with a simple query and build it up
            IQueryable<Application> query = _context.Applications;

            // Include related entities
            query = query.Include(a => a.Migrant).Include(a => a.Officer);

            // Apply filters one by one
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(a => a.Migrant.FullName.ToLower().Contains(searchString.ToLower()));
            }

            if (!string.IsNullOrEmpty(typeFilter))
            {
                query = query.Where(a => a.Type == typeFilter);
            }

            if (statusFilter != null && statusFilter.Length > 0)
            {
                // Use a simpler approach for multiple statuses
                query = query.Where(a => statusFilter.Contains(a.Status));
            }

            // Execute the query
            var applications = await query.ToListAsync();
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
                            : $"Status changed from {existingApplication.Status} to {application.Status}"
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

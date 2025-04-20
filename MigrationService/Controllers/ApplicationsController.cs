using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigrationService.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;

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
        public async Task<IActionResult> Index()
        {
            var applications = await _context.Applications
                .Include(a => a.Migrant)
                .Include(a => a.Officer)
                .ToListAsync();
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
                .FirstOrDefaultAsync(a => a.ApplicationID == id);

            if (application == null)
                return NotFound();

            return View(application);
        }

        // GET: Applications/Create
        public IActionResult Create()
        {
            ViewData["MigrantID"] = new SelectList(_context.Migrants, "MigrantID", "FullName");
            ViewData["OfficerID"] = new SelectList(_context.Officers, "OfficerID", "FullName");
            return View();
        }

        // POST: Applications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MigrantID,OfficerID,Type,Status")] Application application)
        {
            List<ValidationResult> validationResults;
            if (!application.Validate(out validationResults))
            {
                foreach (var result in validationResults)
                {
                    ModelState.AddModelError(string.Join(", ", result.MemberNames), result.ErrorMessage);
                }
                ViewData["MigrantID"] = new SelectList(_context.Migrants, "MigrantID", "FullName", application.MigrantID);
                ViewData["OfficerID"] = new SelectList(_context.Officers, "OfficerID", "FullName", application.OfficerID);
                return View(application);
            }

            try
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
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while creating the application. Please try again.");
                ViewData["MigrantID"] = new SelectList(_context.Migrants, "MigrantID", "FullName", application.MigrantID);
                ViewData["OfficerID"] = new SelectList(_context.Officers, "OfficerID", "FullName", application.OfficerID);
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
            
            ViewData["MigrantID"] = new SelectList(_context.Migrants, "MigrantID", "FullName", application.MigrantID);
            ViewData["OfficerID"] = new SelectList(_context.Officers, "OfficerID", "FullName", application.OfficerID);
            return View(application);
        }

        // POST: Applications/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ApplicationID,MigrantID,OfficerID,Type,Status,SubmissionDate,DecisionDate")] Application application, string statusComment)
        {
            if (id != application.ApplicationID)
            {
                return NotFound();
            }

            List<ValidationResult> validationResults;
            if (!application.Validate(out validationResults))
            {
                foreach (var result in validationResults)
                {
                    ModelState.AddModelError(string.Join(", ", result.MemberNames), result.ErrorMessage);
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
                    return NotFound();
                }

                // Check if status has changed
                if (existingApplication.Status != application.Status)
                {
                    var statusChange = new StatusChange
                    {
                        ApplicationID = application.ApplicationID,
                        Status = application.Status,
                        ChangedAt = DateTime.Now,
                        Comment = statusComment ?? "Status updated"
                    };

                    _context.StatusChanges.Add(statusChange);

                    // Automatically set DecisionDate when status changes to Approved or Rejected
                    if (application.Status == "Approved" || application.Status == "Rejected")
                    {
                        existingApplication.DecisionDate = DateTime.Now;
                    }
                    else if (application.Status == "Pending" || application.Status == "InProgress")
                    {
                        // Clear DecisionDate if status changes back to Pending or InProgress
                        existingApplication.DecisionDate = null;
                    }
                }

                // Update only the changed fields
                existingApplication.MigrantID = application.MigrantID;
                existingApplication.OfficerID = application.OfficerID;
                existingApplication.Type = application.Type;
                existingApplication.Status = application.Status;
                
                // Preserve the time component of the submission date
                var submissionDate = application.SubmissionDate.Date;
                var submissionTime = existingApplication.SubmissionDate.TimeOfDay;
                existingApplication.SubmissionDate = submissionDate.Add(submissionTime);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApplicationExists(application.ApplicationID))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while saving the application. Please try again.");
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
            var application = await _context.Applications.FindAsync(id);
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
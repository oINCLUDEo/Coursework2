using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MigrationService.Models;
using MigrationService.Filters;

namespace MigrationService.Controllers
{
    [RequireAuth]
    public class CertificatesController : Controller
    {
        private readonly FlightSchoolDbContext _context;

        public CertificatesController(FlightSchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int? courseFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name");

            var query = _context.Certificates
                .Include(c => c.Course)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var lowered = searchString.ToLower();
                query = query.Where(c =>
                    (c.Title != null && c.Title.ToLower().Contains(lowered)) ||
                    (c.Description != null && c.Description.ToLower().Contains(lowered)));
            }

            if (courseFilter.HasValue)
            {
                query = query.Where(c => c.CourseID == courseFilter.Value);
            }

            var list = await query
                .OrderBy(c => c.Title)
                .AsNoTracking()
                .ToListAsync();
            return View(list);
        }

        public IActionResult Create(int? courseId)
        {
            ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name", courseId);
            var certificate = new Certificate
            {
                CourseID = courseId
            };
            return View(certificate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Certificate certificate)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name", certificate.CourseID);
                return View(certificate);
            }

            _context.Add(certificate);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Шаблон сертификата создан.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate == null) return NotFound();
            ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name", certificate.CourseID);
            return View(certificate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CertificateID,Title,Description,CourseID,DefaultValidityDays")] Certificate certificate)
        {
            if (id != certificate.CertificateID) return NotFound();
            if (!ModelState.IsValid)
            {
                ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name", certificate.CourseID);
                return View(certificate);
            }

            var existing = await _context.Certificates.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Title = certificate.Title;
            existing.Description = certificate.Description;
            existing.CourseID = certificate.CourseID;
            existing.DefaultValidityDays = certificate.DefaultValidityDays;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Шаблон обновлен.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var certificate = await _context.Certificates
                .Include(c => c.Course)
                .Include(c => c.StudentCertificates)
                    .ThenInclude(sc => sc.Student)
                .FirstOrDefaultAsync(c => c.CertificateID == id);
            if (certificate == null) return NotFound();
            return View(certificate);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var certificate = await _context.Certificates
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.CertificateID == id);
            if (certificate == null) return NotFound();
            return View(certificate);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate != null)
            {
                _context.Certificates.Remove(certificate);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}




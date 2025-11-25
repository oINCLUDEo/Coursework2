using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MigrationService.Filters;
using MigrationService.Models;

namespace MigrationService.Controllers
{
    [RequireAuth]
    public class StudentCertificatesController : Controller
    {
        private readonly FlightSchoolDbContext _context;

        public StudentCertificatesController(FlightSchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int? studentFilter, int? courseFilter, int? certificateFilter, string statusFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewBag.Students = new SelectList(_context.Students.AsNoTracking().ToList(), "StudentID", "FullName");
            ViewBag.Certificates = new SelectList(_context.Certificates.AsNoTracking().ToList(), "CertificateID", "Title");
            ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name");
            ViewBag.StatusFilter = statusFilter;

            var query = _context.StudentCertificates
                .Include(sc => sc.Student)
                .Include(sc => sc.Certificate)
                    .ThenInclude(c => c.Course)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var lowered = searchString.ToLower();
                query = query.Where(sc =>
                    (sc.CertificateNumber != null && sc.CertificateNumber.ToLower().Contains(lowered)) ||
                    (sc.Student != null && sc.Student.FullName != null && sc.Student.FullName.ToLower().Contains(lowered)) ||
                    (sc.Certificate != null && sc.Certificate.Title != null && sc.Certificate.Title.ToLower().Contains(lowered)));
            }

            if (studentFilter.HasValue)
            {
                query = query.Where(sc => sc.StudentID == studentFilter.Value);
            }

            if (courseFilter.HasValue)
            {
                query = query.Where(sc => sc.Certificate != null && sc.Certificate.CourseID == courseFilter.Value);
            }

            if (certificateFilter.HasValue)
            {
                query = query.Where(sc => sc.CertificateID == certificateFilter.Value);
            }

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                query = query.Where(sc => sc.Status == statusFilter);
            }

            var list = await query
                .OrderByDescending(sc => sc.IssuedDate)
                .AsNoTracking()
                .ToListAsync();
            return View(list);
        }

        public IActionResult Create(int? courseId, int? certificateId)
        {
            PopulateLookups(null, courseId, certificateId);
            var model = new StudentCertificate
            {
                IssuedDate = DateTime.Today,
                CertificateID = certificateId ?? 0
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentCertificate studentCertificate)
        {
            var certificateTemplate = await _context.Certificates.FindAsync(studentCertificate.CertificateID);
            if (certificateTemplate == null)
            {
                ModelState.AddModelError("CertificateID", "Выберите шаблон сертификата");
                PopulateLookups(studentCertificate.StudentID, null, studentCertificate.CertificateID);
                return View(studentCertificate);
            }

            if (!ModelState.IsValid)
            {
                PopulateLookups(studentCertificate.StudentID, certificateTemplate.CourseID, studentCertificate.CertificateID);
                TempData["Error"] = "Проверьте правильность заполнения всех обязательных полей.";
                return View(studentCertificate);
            }

            if (!studentCertificate.ValidUntil.HasValue && certificateTemplate.DefaultValidityDays.HasValue)
            {
                studentCertificate.ValidUntil = studentCertificate.IssuedDate.AddDays(certificateTemplate.DefaultValidityDays.Value);
            }

            _context.StudentCertificates.Add(studentCertificate);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Сертификат курсантy выдан.";

            if (certificateTemplate.CourseID.HasValue)
            {
                return RedirectToAction("Details", "Courses", new { id = certificateTemplate.CourseID.Value });
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var studentCertificate = await _context.StudentCertificates
                .Include(sc => sc.Certificate)
                .FirstOrDefaultAsync(sc => sc.StudentCertificateID == id);
            if (studentCertificate == null) return NotFound();
            PopulateLookups(studentCertificate.StudentID, studentCertificate.Certificate?.CourseID, studentCertificate.CertificateID);
            return View(studentCertificate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StudentCertificateID,StudentID,CertificateID,IssuedDate,CertificateNumber,ValidUntil,Status,Notes")] StudentCertificate studentCertificate)
        {
            if (id != studentCertificate.StudentCertificateID) return NotFound();
            if (!ModelState.IsValid)
            {
                PopulateLookups(studentCertificate.StudentID, null, studentCertificate.CertificateID);
                return View(studentCertificate);
            }

            var existing = await _context.StudentCertificates.FindAsync(id);
            if (existing == null) return NotFound();

            existing.StudentID = studentCertificate.StudentID;
            existing.CertificateID = studentCertificate.CertificateID;
            existing.IssuedDate = studentCertificate.IssuedDate;
            existing.CertificateNumber = studentCertificate.CertificateNumber;
            existing.ValidUntil = studentCertificate.ValidUntil;
            existing.Status = studentCertificate.Status;
            existing.Notes = studentCertificate.Notes;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Запись обновлена.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var studentCertificate = await _context.StudentCertificates
                .Include(sc => sc.Student)
                .Include(sc => sc.Certificate)
                    .ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(sc => sc.StudentCertificateID == id);
            if (studentCertificate == null) return NotFound();
            return View(studentCertificate);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var studentCertificate = await _context.StudentCertificates
                .Include(sc => sc.Student)
                .Include(sc => sc.Certificate)
                    .ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(sc => sc.StudentCertificateID == id);
            if (studentCertificate == null) return NotFound();
            return View(studentCertificate);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var studentCertificate = await _context.StudentCertificates.FindAsync(id);
            if (studentCertificate != null)
            {
                _context.StudentCertificates.Remove(studentCertificate);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private void PopulateLookups(object? selectedStudent, int? courseId, object? selectedCertificate)
        {
            ViewBag.Students = new SelectList(_context.Students.AsNoTracking().ToList(), "StudentID", "FullName", selectedStudent);

            var certificatesQuery = _context.Certificates
                .Include(c => c.Course)
                .AsNoTracking();
            if (courseId.HasValue)
            {
                certificatesQuery = certificatesQuery.Where(c => c.CourseID == courseId.Value);
            }

            var certificates = certificatesQuery
                .OrderBy(c => c.Title)
                .Select(c => new
                {
                    c.CertificateID,
                    Title = c.Course != null && !string.IsNullOrEmpty(c.Course.Name)
                        ? $"{c.Title} ({c.Course.Name})"
                        : c.Title
                })
                .ToList();

            ViewBag.Certificates = new SelectList(certificates, "CertificateID", "Title", selectedCertificate);
        }
    }
}


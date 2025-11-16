using System;
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
    public class StudentsController : Controller
    {
        private readonly FlightSchoolDbContext _context;

        public StudentsController(FlightSchoolDbContext context)
        {
            _context = context;
        }

        // GET: Students
        public async Task<IActionResult> Index(string searchString, int? courseFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name");

            var query = _context.Students
                .Include(s => s.StudentCourses)
                    .ThenInclude(sc => sc.Course)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var lowered = searchString.ToLower();
                query = query.Where(s =>
                    (s.FullName != null && s.FullName.ToLower().Contains(lowered)) ||
                    (s.Email != null && s.Email.ToLower().Contains(lowered)) ||
                    (s.Phone != null && s.Phone.ToLower().Contains(lowered))
                );
            }

            if (courseFilter.HasValue)
            {
                query = query.Where(s => s.StudentCourses.Any(sc => sc.CourseID == courseFilter.Value));
            }

            var students = await query.AsNoTracking().ToListAsync();
            return View(students);
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .Include(s => s.StudentCourses)
                    .ThenInclude(sc => sc.Course)
                .FirstOrDefaultAsync(m => m.StudentID == id);
            if (student == null) return NotFound();
            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Email,Phone,Address,BirthDate,MedicalCertificateNo,EnrollmentDate,Notes")] Student student)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrWhiteSpace(student.Email))
                {
                    if (await _context.Students.AnyAsync(s => s.Email == student.Email))
                    {
                        ModelState.AddModelError("Email", "Курсант с таким Email уже существует.");
                        return View(student);
                    }
                }

                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();
            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StudentID,FullName,Email,Phone,Address,BirthDate,MedicalCertificateNo,EnrollmentDate,Notes")] Student student)
        {
            if (id != student.StudentID) return NotFound();
            if (!ModelState.IsValid) return View(student);

            if (!string.IsNullOrWhiteSpace(student.Email))
            {
                if (await _context.Students.AnyAsync(s => s.Email == student.Email && s.StudentID != id))
                {
                    ModelState.AddModelError("Email", "Курсант с таким Email уже существует.");
                    return View(student);
                }
            }

            var existing = await _context.Students.FindAsync(id);
            if (existing == null) return NotFound();

            existing.FullName = student.FullName;
            existing.Email = student.Email;
            existing.Phone = student.Phone;
            existing.Address = student.Address;
            existing.BirthDate = student.BirthDate;
            existing.MedicalCertificateNo = student.MedicalCertificateNo;
            existing.EnrollmentDate = student.EnrollmentDate;
            existing.Notes = student.Notes;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Students.FirstOrDefaultAsync(m => m.StudentID == id);
            if (student == null) return NotFound();
            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students
                .Include(s => s.Lessons)
                .Include(s => s.StudentCourses)
                .FirstOrDefaultAsync(s => s.StudentID == id);
            if (student == null) return RedirectToAction(nameof(Index));

            if (student.Lessons.Any())
            {
                ModelState.AddModelError(string.Empty, "Нельзя удалить курсанта с занятиями.");
                return View("Delete", student);
            }

            _context.StudentCourses.RemoveRange(student.StudentCourses);
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}



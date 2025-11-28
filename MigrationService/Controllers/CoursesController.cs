using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigrationService.Models;
using MigrationService.Filters;

namespace MigrationService.Controllers
{
    [RequireAuth]
    public class CoursesController : Controller
    {
        private readonly FlightSchoolDbContext _context;

        public CoursesController(FlightSchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search, bool? onlyActive)
        {
            ViewData["Search"] = search;
            ViewData["OnlyActive"] = onlyActive;
            var query = _context.Courses.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(c => (c.Name != null && c.Name.ToLower().Contains(s)) || (c.Category != null && c.Category.ToLower().Contains(s)));
            }
            if (onlyActive == true)
                query = query.Where(c => c.IsActive);
            return View(await query.AsNoTracking().ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            
            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CourseID == id);
            
            if (course == null) return NotFound();

            // Загружаем связанные данные
            var lessons = await _context.Lessons
                .Include(l => l.Student)
                .Include(l => l.Instructor)
                .Include(l => l.Aircraft)
                .Where(l => l.CourseID == id)
                .OrderByDescending(l => l.Date)
                .AsNoTracking()
                .ToListAsync();

            var exams = await _context.Exams
                .Include(e => e.Student)
                .Include(e => e.Instructor)
                .Where(e => e.CourseID == id)
                .OrderByDescending(e => e.Date)
                .AsNoTracking()
                .ToListAsync();

            var studentCertificates = await _context.StudentCertificates
                .Include(sc => sc.Student)
                .Include(sc => sc.Certificate)
                .Where(sc => sc.Certificate.CourseID == id)
                .OrderByDescending(sc => sc.IssuedDate)
                .AsNoTracking()
                .ToListAsync();

            var students = await _context.Students
                .Where(s => s.CourseID == id)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Lessons = lessons;
            ViewBag.Exams = exams;
            ViewBag.StudentCertificates = studentCertificates;
            ViewBag.Students = students;
            ViewBag.CourseId = id;

            return View(course);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Category,Description,RequiredHours,IsActive")] Course course)
        {
            if (!ModelState.IsValid) return View(course);
            _context.Add(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CourseID,Name,Category,Description,RequiredHours,IsActive")] Course course)
        {
            if (id != course.CourseID) return NotFound();
            if (!ModelState.IsValid) return View(course);
            var existing = await _context.Courses.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Name = course.Name;
            existing.Category = course.Category;
            existing.Description = course.Description;
            existing.RequiredHours = course.RequiredHours;
            existing.IsActive = course.IsActive;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseID == id);
            if (course == null) return NotFound();
            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return RedirectToAction(nameof(Index));

            var hasStudents = await _context.Students.AnyAsync(s => s.CourseID == id);
            var hasLessons = await _context.Lessons.AnyAsync(l => l.CourseID == id);
            var hasExams = await _context.Exams.AnyAsync(e => e.CourseID == id);

            if (hasStudents || hasLessons || hasExams)
            {
                TempData["ErrorMessage"] = "Нельзя удалить курс, пока к нему привязаны студенты, занятия или экзамены. Сначала удалите или перепривяжите дочерние записи.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}

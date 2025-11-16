using System.Linq;
using System.Threading.Tasks;
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
            if (course != null)
            {
                var used = await _context.StudentCourses.AnyAsync(sc => sc.CourseID == id) || await _context.Lessons.AnyAsync(l => l.CourseID == id);
                if (used)
                {
                    ModelState.AddModelError(string.Empty, "Нельзя удалить курс, используемый в зачислениях или занятиях.");
                    return View("Delete", course);
                }
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}



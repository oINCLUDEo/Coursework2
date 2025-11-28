using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigrationService.Models;
using MigrationService.Filters;

namespace MigrationService.Controllers
{
    [RequireAuth]
    public class InstructorsController : Controller
    {
        private readonly FlightSchoolDbContext _context;

        public InstructorsController(FlightSchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, bool? onlyActive)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["OnlyActive"] = onlyActive;

            var query = _context.Instructors.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var lowered = searchString.ToLower();
                query = query.Where(i =>
                    (i.FullName != null && i.FullName.ToLower().Contains(lowered)) ||
                    (i.Email != null && i.Email.ToLower().Contains(lowered))
                );
            }
            if (onlyActive == true)
            {
                query = query.Where(i => i.IsActive);
            }
            return View(await query.AsNoTracking().ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var instructor = await _context.Instructors.FirstOrDefaultAsync(m => m.InstructorID == id);
            if (instructor == null) return NotFound();
            return View(instructor);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Email,Phone,Rank,HireDate,IsActive")] Instructor instructor)
        {
            if (!ModelState.IsValid) return View(instructor);

            if (!string.IsNullOrWhiteSpace(instructor.Email) && await _context.Instructors.AnyAsync(i => i.Email == instructor.Email))
            {
                ModelState.AddModelError("Email", "Инструктор с таким Email уже существует.");
                return View(instructor);
            }
            _context.Add(instructor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var instructor = await _context.Instructors.FindAsync(id);
            if (instructor == null) return NotFound();
            return View(instructor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("InstructorID,FullName,Email,Phone,Rank,HireDate,IsActive")] Instructor instructor)
        {
            if (id != instructor.InstructorID) return NotFound();
            if (!ModelState.IsValid) return View(instructor);

            if (!string.IsNullOrWhiteSpace(instructor.Email) && await _context.Instructors.AnyAsync(i => i.Email == instructor.Email && i.InstructorID != id))
            {
                ModelState.AddModelError("Email", "Инструктор с таким Email уже существует.");
                return View(instructor);
            }

            var existing = await _context.Instructors.FindAsync(id);
            if (existing == null) return NotFound();
            existing.FullName = instructor.FullName;
            existing.Email = instructor.Email;
            existing.Phone = instructor.Phone;
            existing.Rank = instructor.Rank;
            existing.HireDate = instructor.HireDate;
            existing.IsActive = instructor.IsActive;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var instructor = await _context.Instructors.FirstOrDefaultAsync(m => m.InstructorID == id);
            if (instructor == null) return NotFound();
            return View(instructor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var instructor = await _context.Instructors.FindAsync(id);
            if (instructor != null)
            {
                var hasLessons = await _context.Lessons.AnyAsync(l => l.InstructorID == id);
                if (hasLessons)
                {
                    TempData["ErrorMessage"] = "Нельзя удалить инструктора, пока за ним закреплены занятия. Сначала удалите или перепривяжите связанные записи.";
                    return RedirectToAction(nameof(Delete), new { id });
                }
                _context.Instructors.Remove(instructor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

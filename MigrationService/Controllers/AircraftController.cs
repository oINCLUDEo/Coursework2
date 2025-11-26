using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigrationService.Models;
using MigrationService.Filters;

namespace MigrationService.Controllers
{
    [RequireAuth]
    public class AircraftController : Controller
    {
        private readonly FlightSchoolDbContext _context;

        public AircraftController(FlightSchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string status, string search)
        {
            ViewData["Status"] = status;
            ViewData["Search"] = search;
            var query = _context.Aircraft.AsQueryable();
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(a => a.Status == status);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(a => (a.TailNumber != null && a.TailNumber.ToLower().Contains(s)) || (a.Model != null && a.Model.ToLower().Contains(s)));
            }
            return View(await query.AsNoTracking().ToListAsync());
        }
        
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var aircraft = await _context.Aircraft.FirstOrDefaultAsync(m => m.AircraftID == id);
            if (aircraft == null) return NotFound();
            return View(aircraft);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TailNumber,Model,Type,Year,TotalHours,Status")] Aircraft aircraft)
        {
            if (!ModelState.IsValid) return View(aircraft);
            if (!string.IsNullOrWhiteSpace(aircraft.TailNumber) && await _context.Aircraft.AnyAsync(a => a.TailNumber == aircraft.TailNumber))
            {
                ModelState.AddModelError("TailNumber", "Самолет с таким бортовым номером уже существует.");
                return View(aircraft);
            }
            _context.Add(aircraft);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var aircraft = await _context.Aircraft.FindAsync(id);
            if (aircraft == null) return NotFound();
            return View(aircraft);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AircraftID,TailNumber,Model,Type,Year,TotalHours,Status")] Aircraft aircraft)
        {
            if (id != aircraft.AircraftID) return NotFound();
            if (!ModelState.IsValid) return View(aircraft);
            if (!string.IsNullOrWhiteSpace(aircraft.TailNumber) && await _context.Aircraft.AnyAsync(a => a.TailNumber == aircraft.TailNumber && a.AircraftID != id))
            {
                ModelState.AddModelError("TailNumber", "Самолет с таким бортовым номером уже существует.");
                return View(aircraft);
            }

            var existing = await _context.Aircraft.FindAsync(id);
            if (existing == null) return NotFound();
            existing.TailNumber = aircraft.TailNumber;
            existing.Model = aircraft.Model;
            existing.Type = aircraft.Type;
            existing.Year = aircraft.Year;
            existing.TotalHours = aircraft.TotalHours;
            existing.Status = aircraft.Status;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var aircraft = await _context.Aircraft.FirstOrDefaultAsync(a => a.AircraftID == id);
            if (aircraft == null) return NotFound();
            return View(aircraft);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aircraft = await _context.Aircraft.FindAsync(id);
            if (aircraft != null)
            {
                var inUse = await _context.Lessons.AnyAsync(l => l.AircraftID == id);
                if (inUse)
                {
                    TempData["ErrorMessage"] = "Нельзя удалить самолет, пока он используется в занятиях. Сначала удалите или перепривяжите связанные записи.";
                    return RedirectToAction(nameof(Delete), new { id });
                }
                _context.Aircraft.Remove(aircraft);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}



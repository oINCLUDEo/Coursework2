using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigrationService.Models;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MigrationService.Controllers
{
    public class OfficersController : Controller
    {
        private readonly MigrationDbContext _context;

        public OfficersController(MigrationDbContext context)
        {
            _context = context;
        }

        // GET: Officers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Officers.ToListAsync());
        }

        // GET: Officers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var officer = await _context.Officers
                .Include(o => o.Applications)
                    .ThenInclude(a => a.Migrant)
                .FirstOrDefaultAsync(o => o.OfficerID == id);

            if (officer == null)
                return NotFound();

            return View(officer);
        }

        // GET: Officers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Officers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Position,Email,Login,Password")] Officer officer)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Officers.AnyAsync(o => o.Email == officer.Email))
                {
                    ModelState.AddModelError("Email", "An officer with this email already exists.");
                    return View(officer);
                }

                if (await _context.Officers.AnyAsync(o => o.Login == officer.Login))
                {
                    ModelState.AddModelError("Login", "An officer with this login already exists.");
                    return View(officer);
                }

                _context.Add(officer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(officer);
        }

        // GET: Officers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var officer = await _context.Officers.FindAsync(id);
            if (officer == null)
                return NotFound();

            return View(officer);
        }

        // POST: Officers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OfficerID,FullName,Position,Email,Login,Password")] Officer officer)
        {
            if (id != officer.OfficerID)
                return NotFound();

            if (ModelState.IsValid)
            {
                if (await _context.Officers.AnyAsync(o => o.Email == officer.Email && o.OfficerID != id))
                {
                    ModelState.AddModelError("Email", "An officer with this email already exists.");
                    return View(officer);
                }

                if (await _context.Officers.AnyAsync(o => o.Login == officer.Login && o.OfficerID != id))
                {
                    ModelState.AddModelError("Login", "An officer with this login already exists.");
                    return View(officer);
                }

                var existingOfficer = await _context.Officers.FindAsync(id);
                if (existingOfficer == null)
                    return NotFound();

                existingOfficer.FullName = officer.FullName;
                existingOfficer.Position = officer.Position;
                existingOfficer.Email = officer.Email;
                existingOfficer.Login = officer.Login;
                existingOfficer.Password = officer.Password;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(officer);
        }

        // GET: Officers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var officer = await _context.Officers
                .Include(o => o.Applications)
                .FirstOrDefaultAsync(m => m.OfficerID == id);
            if (officer == null)
                return NotFound();

            return View(officer);
        }

        // POST: Officers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var officer = await _context.Officers
                .Include(o => o.Applications)
                .FirstOrDefaultAsync(o => o.OfficerID == id);

            if (officer == null)
                return NotFound();

            _context.Officers.Remove(officer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OfficerExists(int id)
        {
            return _context.Officers.Any(e => e.OfficerID == id);
        }
    }
} 

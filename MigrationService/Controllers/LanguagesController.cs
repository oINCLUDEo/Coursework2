using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigrationService.Models;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MigrationService.Controllers
{
    public class LanguagesController : Controller
    {
        private readonly MigrationDbContext _context;

        public LanguagesController(MigrationDbContext context)
        {
            _context = context;
        }

        // GET: Languages
        public async Task<IActionResult> Index()
        {
            return View(await _context.Languages.ToListAsync());
        }

        // GET: Languages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var language = await _context.Languages
                .Include(l => l.MigrantLanguages)
                    .ThenInclude(ml => ml.Migrant)
                .FirstOrDefaultAsync(l => l.LanguageID == id);

            if (language == null)
                return NotFound();

            return View(language);
        }

        // GET: Languages/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Languages/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LanguageName")] Language language)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Languages.AnyAsync(l => l.LanguageName == language.LanguageName))
                {
                    ModelState.AddModelError("LanguageName", "A language with this name already exists.");
                    return View(language);
                }

                _context.Add(language);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(language);
        }

        // GET: Languages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var language = await _context.Languages.FindAsync(id);
            if (language == null)
                return NotFound();

            return View(language);
        }

        // POST: Languages/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LanguageID,LanguageName")] Language language)
        {
            if (id != language.LanguageID)
                return NotFound();

            if (ModelState.IsValid)
            {
                if (await _context.Languages.AnyAsync(l => l.LanguageName == language.LanguageName && l.LanguageID != id))
                {
                    ModelState.AddModelError("LanguageName", "A language with this name already exists.");
                    return View(language);
                }

                var existingLanguage = await _context.Languages.FindAsync(id);
                if (existingLanguage == null)
                    return NotFound();

                existingLanguage.LanguageName = language.LanguageName;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(language);
        }

        // GET: Languages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var language = await _context.Languages
                .Include(l => l.MigrantLanguages)
                .FirstOrDefaultAsync(m => m.LanguageID == id);
            if (language == null)
                return NotFound();

            return View(language);
        }

        // POST: Languages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var language = await _context.Languages
                .Include(l => l.MigrantLanguages)
                .FirstOrDefaultAsync(l => l.LanguageID == id);

            if (language == null)
                return NotFound();

            _context.Languages.Remove(language);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LanguageExists(int id)
        {
            return _context.Languages.Any(e => e.LanguageID == id);
        }
    }
} 

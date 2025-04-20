using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigrationService.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MigrationService.Controllers
{
    public class MigrantsController : Controller
    {
        private readonly MigrationDbContext _context;

        public MigrantsController(MigrationDbContext context)
        {
            _context = context;
        }

        // GET: Migrants/Create
        public IActionResult Create()
        {
            var viewModel = new MigrantCreateViewModel
            {
                Migrant = new Migrant(),
                Countries = _context.Countries.ToList(),
                Languages = _context.Languages.ToList(),
                SelectedLanguageProficiencies = new List<LanguageProficiencyEntry>()
            };

            return View(viewModel);
        }

        // POST: Migrants/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MigrantCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {   
                Console.WriteLine("Модель невалидна.");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }

                viewModel.Countries = _context.Countries.ToList();
                viewModel.Languages = _context.Languages.ToList();
                return View(viewModel);
            }

            _context.Migrants.Add(viewModel.Migrant);
            await _context.SaveChangesAsync();

            // Сохраняем языки
            if (viewModel.SelectedLanguageProficiencies.Any())
            {
                foreach (var entry in viewModel.SelectedLanguageProficiencies)
                {
                    var migrantLanguage = new MigrantLanguage
                    {
                        MigrantID = viewModel.Migrant.MigrantID,
                        LanguageID = entry.LanguageID,
                        ProficiencyLevel = entry.ProficiencyLevel
                    };
                    _context.MigrantLanguages.Add(migrantLanguage);
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        // Метод для отображения списка мигрантов
        public async Task<IActionResult> Index()
        {
            var migrants = await _context.Migrants.ToListAsync();
            return View(migrants);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();
        
            var migrant = await _context.Migrants
                .Include(m => m.Country)
                .Include(m => m.MigrantLanguages)
                .ThenInclude(ml => ml.Language)
                .FirstOrDefaultAsync(m => m.MigrantID == id);
        
            if (migrant == null)
                return NotFound();
        
            var countries = _context.Countries.ToList();
            var languages = _context.Languages.ToList();
        
            // Список стран для выпадающего списка
            ViewBag.CountryList = new SelectList(countries, "CountryID", "CountryName", migrant.CountryID);
            
            // Выбранные языки для мигранта
            var selectedLanguages = migrant.MigrantLanguages.Select(ml => ml.LanguageID).ToArray();
            ViewBag.Languages = languages;
            ViewBag.SelectedLanguages = selectedLanguages;
        
            return View(migrant);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Migrant migrant, int[] selectedLanguages)
        {
            if (id != migrant.MigrantID)
                return NotFound();
        
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(migrant);
        
                    // Обновляем языки мигранта
                    var currentLanguages = _context.MigrantLanguages.Where(ml => ml.MigrantID == migrant.MigrantID);
                    _context.MigrantLanguages.RemoveRange(currentLanguages);
        
                    if (selectedLanguages != null)
                    {
                        foreach (var languageId in selectedLanguages)
                        {
                            var migrantLanguage = new MigrantLanguage
                            {
                                MigrantID = migrant.MigrantID,
                                LanguageID = languageId
                            };
                            _context.MigrantLanguages.Add(migrantLanguage);
                        }
                    }
        
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Migrants.Any(e => e.MigrantID == migrant.MigrantID))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(migrant);
        }

        // Метод для удаления мигранта
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var migrant = await _context.Migrants.FirstOrDefaultAsync(m => m.MigrantID == id);
            if (migrant == null) return NotFound();

            return View(migrant);
        }

        // Метод для подтверждения удаления мигранта
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var migrant = await _context.Migrants.FindAsync(id);
            _context.Migrants.Remove(migrant);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}

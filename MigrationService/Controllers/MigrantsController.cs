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
        public async Task<IActionResult> Index(string searchString, int? countryFilter, int? languageFilter)
        {
            // Store the current filter values in ViewData for the view
            ViewData["CurrentFilter"] = searchString;
            
            // Get all countries and languages for the filter dropdowns
            ViewBag.Countries = new SelectList(_context.Countries, "CountryID", "CountryName");
            ViewBag.Languages = new SelectList(_context.Languages, "LanguageID", "LanguageName");
            
            // Start with the base query including related data
            var query = _context.Migrants
                .Include(m => m.Country)
                .Include(m => m.MigrantLanguages)
                .ThenInclude(ml => ml.Language)
                .AsQueryable();
            
            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(m => 
                    m.FullName.ToLower().Contains(searchString) || 
                    m.PassportNumber.ToLower().Contains(searchString) || 
                    m.Address.ToLower().Contains(searchString));
            }
            
            // Apply country filter if provided
            if (countryFilter.HasValue)
            {
                query = query.Where(m => m.CountryID == countryFilter.Value);
            }
            
            // Apply language filter if provided
            if (languageFilter.HasValue)
            {
                query = query.Where(m => m.MigrantLanguages.Any(ml => ml.LanguageID == languageFilter.Value));
            }
            
            // Execute the query and get the results
            var migrants = await query.ToListAsync();
            
            return View(migrants);
        }

        // Метод для редактирования данных мигранта
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
        public async Task<IActionResult> Edit(int id, Migrant migrant, int[] selectedLanguages, Dictionary<int, string> proficiencyLevels)
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
                                LanguageID = languageId,
                                ProficiencyLevel = proficiencyLevels.ContainsKey(languageId) ? proficiencyLevels[languageId] : "Начальный"
                            };
                            _context.MigrantLanguages.Add(migrantLanguage);
                        }
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Migrants.Any(e => e.MigrantID == migrant.MigrantID))
                        return NotFound();
                    else
                        throw;
                }
            }

            // If we got this far, something failed, redisplay form
            var countries = _context.Countries.ToList();
            var languages = _context.Languages.ToList();
            ViewBag.CountryList = new SelectList(countries, "CountryID", "CountryName", migrant.CountryID);
            ViewBag.Languages = languages;
            ViewBag.SelectedLanguages = selectedLanguages ?? Array.Empty<int>();
            return View(migrant);
        }

        // Метод для отображения страницы подтверждения удаления
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var migrant = await _context.Migrants
                .FirstOrDefaultAsync(m => m.MigrantID == id);

            if (migrant == null)
                return NotFound();

            return View(migrant);
        }

        // Метод для подтверждения удаления мигранта
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Отладочная информация
            Console.WriteLine($"Попытка удалить мигранта с ID: {id}");

            var migrant = await _context.Migrants.FindAsync(id);
            if (migrant == null)
            {
                // Мигрант не найден
                Console.WriteLine("Мигрант не найден в базе данных.");
                return NotFound();
            }

            // Удаление мигранта
            _context.Migrants.Remove(migrant);
            await _context.SaveChangesAsync();

            // Перенаправление на список мигрантов после удаления
            return RedirectToAction(nameof(Index));
        }

        // GET: Migrants/Details/5
        public async Task<IActionResult> Details(int? id)
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

            return View(migrant);
        }
    }
}

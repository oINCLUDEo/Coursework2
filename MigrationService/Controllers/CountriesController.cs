using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigrationService.Models;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MigrationService.Controllers
{
    public class CountriesController : Controller
    {
        private readonly MigrationDbContext _context;

        public CountriesController(MigrationDbContext context)
        {
            _context = context;
        }

        // GET: Countries
        public async Task<IActionResult> Index()
        {
            return View(await _context.Countries.ToListAsync());
        }

        // GET: Countries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var country = await _context.Countries
                .Include(c => c.Migrants)
                .FirstOrDefaultAsync(c => c.CountryID == id);

            if (country == null)
                return NotFound();

            return View(country);
        }

        // GET: Countries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Countries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CountryName,ISOCode,VisaRequired")] Country country)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(country.CountryName))
                {
                    ModelState.AddModelError("CountryName", "Country name is required");
                }
                if (string.IsNullOrWhiteSpace(country.ISOCode))
                {
                    ModelState.AddModelError("ISOCode", "ISO code is required");
                }

                if (ModelState.IsValid)
                {
                    // Check if country with same name or code already exists
                    if (await _context.Countries.AnyAsync(c => c.CountryName == country.CountryName))
                    {
                        ModelState.AddModelError("CountryName", "A country with this name already exists.");
                        return View(country);
                    }

                    if (await _context.Countries.AnyAsync(c => c.ISOCode == country.ISOCode))
                    {
                        ModelState.AddModelError("ISOCode", "A country with this code already exists.");
                        return View(country);
                    }

                    _context.Add(country);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while creating the country. Please try again.");
            }
            return View(country);
        }

        // GET: Countries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var country = await _context.Countries.FindAsync(id);
            if (country == null)
                return NotFound();

            return View(country);
        }

        // POST: Countries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CountryID,CountryName,ISOCode,VisaRequired")] Country country)
        {
            if (id != country.CountryID)
                return NotFound();

            try
            {
                if (string.IsNullOrWhiteSpace(country.CountryName))
                {
                    ModelState.AddModelError("CountryName", "Country name is required");
                }
                if (string.IsNullOrWhiteSpace(country.ISOCode))
                {
                    ModelState.AddModelError("ISOCode", "ISO code is required");
                }

                if (ModelState.IsValid)
                {
                    // Check if country with same name or code already exists (excluding current country)
                    if (await _context.Countries.AnyAsync(c => c.CountryName == country.CountryName && c.CountryID != id))
                    {
                        ModelState.AddModelError("CountryName", "A country with this name already exists.");
                        return View(country);
                    }

                    if (await _context.Countries.AnyAsync(c => c.ISOCode == country.ISOCode && c.CountryID != id))
                    {
                        ModelState.AddModelError("ISOCode", "A country with this code already exists.");
                        return View(country);
                    }

                    var existingCountry = await _context.Countries.FindAsync(id);
                    if (existingCountry == null)
                        return NotFound();

                    existingCountry.CountryName = country.CountryName;
                    existingCountry.ISOCode = country.ISOCode;
                    existingCountry.VisaRequired = country.VisaRequired;
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CountryExists(country.CountryID))
                    return NotFound();
                else
                    throw;
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while saving the country. Please try again.");
            }
            return View(country);
        }

        // GET: Countries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var country = await _context.Countries
                .Include(c => c.Migrants)
                .FirstOrDefaultAsync(m => m.CountryID == id);
            if (country == null)
                return NotFound();

            return View(country);
        }

        // POST: Countries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var country = await _context.Countries
                .Include(c => c.Migrants)
                .FirstOrDefaultAsync(c => c.CountryID == id);

            if (country == null)
                return NotFound();

            try
            {
                _context.Countries.Remove(country);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while deleting the country. Please try again.");
                return View("Delete", country);
            }
        }

        private bool CountryExists(int id)
        {
            return _context.Countries.Any(e => e.CountryID == id);
        }
    }
} 
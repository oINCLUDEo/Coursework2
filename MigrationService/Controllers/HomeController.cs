using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using MigrationService.Models;

namespace MigrationService.Controllers
{
    public class HomeController : Controller
    {
        private readonly MigrationDbContext _context;

        public HomeController(MigrationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Get counts for dashboard
            ViewBag.MigrantsCount = _context.Migrants.Count();
            ViewBag.OfficersCount = _context.Officers.Count();
            ViewBag.ApplicationsCount = _context.Applications.Count();
            ViewBag.CountriesCount = _context.Countries.Count();
            ViewBag.LanguagesCount = _context.Languages.Count();
            ViewBag.DocumentsCount = _context.Documents.Count();
            
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
} 
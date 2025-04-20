using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigrationService.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MigrationService.Controllers
{
    public class SearchController : Controller
    {
        private readonly MigrationDbContext _context;

        public SearchController(MigrationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new SearchResults());
            }

            // Execute each query separately to avoid SQL syntax issues
            var migrants = await _context.Migrants
                .Where(m => m.FullName.Contains(query) || 
                           m.PassportNumber.Contains(query) ||
                           m.PhoneNumber.Contains(query))
                .Take(5)
                .ToListAsync();

            var applications = await _context.Applications
                .Include(a => a.Migrant)
                .Include(a => a.Officer)
                .Where(a => a.Migrant.FullName.Contains(query) || 
                           a.Type.Contains(query) ||
                           a.Status.Contains(query))
                .Take(5)
                .ToListAsync();

            var documents = await _context.Documents
                .Include(d => d.Application)
                    .ThenInclude(a => a.Migrant)
                .Where(d => d.FileName.Contains(query) || 
                           d.FileType.Contains(query) ||
                           d.Application.Migrant.FullName.Contains(query))
                .Take(5)
                .ToListAsync();

            var searchResults = new SearchResults
            {
                Query = query,
                Migrants = migrants.AsQueryable(),
                Applications = applications.AsQueryable(),
                Documents = documents.AsQueryable()
            };

            return View(searchResults);
        }
    }

    public class SearchResults
    {
        public string Query { get; set; }
        public IQueryable<Migrant> Migrants { get; set; }
        public IQueryable<Application> Applications { get; set; }
        public IQueryable<Document> Documents { get; set; }
    }
} 
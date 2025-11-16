using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using MigrationService.Models;
using MigrationService.Filters;

namespace MigrationService.Controllers
{
    [RequireAuth]
    public class HomeController : Controller
    {
        private readonly FlightSchoolDbContext _context;

        public HomeController(FlightSchoolDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.StudentsCount = _context.Students.Count();
            ViewBag.InstructorsCount = _context.Instructors.Count();
            ViewBag.CoursesCount = _context.Courses.Count();
            ViewBag.AircraftCount = _context.Aircraft.Count();
            ViewBag.LessonsCount = _context.Lessons.Count();
            ViewBag.ExamsCount = _context.Exams.Count();
            ViewBag.CertificatesCount = _context.Certificates.Count();
            
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
} 
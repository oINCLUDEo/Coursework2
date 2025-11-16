using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using MigrationService.Models;
using MigrationService.Filters;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IActionResult> Index()
        {
            ViewBag.StudentsCount = _context.Students.Count();
            ViewBag.InstructorsCount = _context.Instructors.Count();
            ViewBag.CoursesCount = _context.Courses.Count();
            ViewBag.AircraftCount = _context.Aircraft.Count();
            ViewBag.LessonsCount = _context.Lessons.Count();
            ViewBag.ExamsCount = _context.Exams.Count();
            ViewBag.CertificatesCount = _context.Certificates.Count();

            // Использование хранимой процедуры: sp_GetUpcomingLessons
            // Получаем предстоящие занятия на ближайшие 7 дней
            var upcomingLessons = await _context.Database
                .SqlQueryRaw<UpcomingLessonResult>(
                    "EXEC sp_GetUpcomingLessons @daysAhead = {0}", 7)
                .ToListAsync();

            ViewBag.UpcomingLessons = upcomingLessons;
            
            return View();
        }

        // Класс для результатов хранимой процедуры
        public class UpcomingLessonResult
        {
            public int LessonID { get; set; }
            public DateTime Date { get; set; }
            public decimal DurationHours { get; set; }
            public string Topic { get; set; }
            public string Status { get; set; }
            public string StudentName { get; set; }
            public string InstructorName { get; set; }
            public string TailNumber { get; set; }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
} 
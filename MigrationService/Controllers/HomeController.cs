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
            // Счетчики записей на каждой странице/сущности
            ViewBag.StudentsCount = _context.Students.Count();
            ViewBag.InstructorsCount = _context.Instructors.Count();
            ViewBag.CoursesCount = _context.Courses.Count();
            ViewBag.AircraftCount = _context.Aircraft.Count();
            ViewBag.LessonsCount = _context.Lessons.Count();
            ViewBag.ExamsCount = _context.Exams.Count();
            ViewBag.CertificatesCount = _context.StudentCertificates.Count();

            var startDate = DateTime.Today;
            var endDate = startDate.AddDays(7);

            var upcomingLessons = await _context.Lessons
                .AsNoTracking()
                .Include(l => l.Student)
                .Include(l => l.Instructor)
                .Include(l => l.Aircraft)
                .Where(l => l.Date >= startDate && l.Date < endDate)
                .OrderBy(l => l.Date)
                .Take(200)
                .Select(l => new UpcomingLessonResult
                {
                    LessonID = l.LessonID,
                    Date = l.Date,
                    DurationHours = l.DurationHours,
                    Topic = l.Topic,
                    Status = l.Status,
                    StudentName = l.Student.FullName,
                    InstructorName = l.Instructor.FullName,
                    TailNumber = l.Aircraft != null ? l.Aircraft.TailNumber : null
                })
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
            public string Topic { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string StudentName { get; set; } = string.Empty;
            public string InstructorName { get; set; } = string.Empty;
            public string? TailNumber { get; set; }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
} 
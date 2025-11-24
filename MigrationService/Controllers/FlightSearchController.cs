using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigrationService.Models;
using MigrationService.Filters;

namespace MigrationService.Controllers
{
    [RequireAuth]
    public class FlightSearchController : Controller
    {
        private readonly FlightSchoolDbContext _context;

        public FlightSearchController(FlightSchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new FlightSearchResults());
            }

            var lowered = query.ToLower();

            var students = await _context.Students
                .Where(s => (s.FullName != null && s.FullName.ToLower().Contains(lowered)) || (s.Email != null && s.Email.ToLower().Contains(lowered)))
                .Take(5).ToListAsync();

            var instructors = await _context.Instructors
                .Where(i => (i.FullName != null && i.FullName.ToLower().Contains(lowered)) || (i.Email != null && i.Email.ToLower().Contains(lowered)))
                .Take(5).ToListAsync();

            var lessons = await _context.Lessons
                .Include(l => l.Student)
                .Include(l => l.Instructor)
                .Where(l => (l.Topic != null && l.Topic.ToLower().Contains(lowered)) || (l.Student.FullName != null && l.Student.FullName.ToLower().Contains(lowered)))
                .Take(5).ToListAsync();

            var aircraft = await _context.Aircraft
                .Where(a => (a.TailNumber != null && a.TailNumber.ToLower().Contains(lowered)) || (a.Model != null && a.Model.ToLower().Contains(lowered)))
                .Take(5).ToListAsync();

            return View(new FlightSearchResults
            {
                Query = query,
                Students = students,
                Instructors = instructors,
                Lessons = lessons,
                Aircraft = aircraft
            });
        }
    }

    public class FlightSearchResults
    {
        public string Query { get; set; } = string.Empty;
        public System.Collections.Generic.List<Student> Students { get; set; } = new();
        public System.Collections.Generic.List<Instructor> Instructors { get; set; } = new();
        public System.Collections.Generic.List<Lesson> Lessons { get; set; } = new();
        public System.Collections.Generic.List<Aircraft> Aircraft { get; set; } = new();
    }
}



using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MigrationService.Models;
using MigrationService.Filters;

namespace MigrationService.Controllers
{
    [RequireAuth]
    public class LessonsController : Controller
    {
        private readonly FlightSchoolDbContext _context;

        public LessonsController(FlightSchoolDbContext context)
        {
            _context = context;
        }

        private void PopulateLookups(object? selectedStudent = null, object? selectedInstructor = null, object? selectedCourse = null, object? selectedAircraft = null)
        {
            ViewBag.Students = new SelectList(_context.Students.AsNoTracking().ToList(), "StudentID", "FullName", selectedStudent);
            ViewBag.Instructors = new SelectList(_context.Instructors.AsNoTracking().ToList(), "InstructorID", "FullName", selectedInstructor);
            ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name", selectedCourse);
            ViewBag.Aircraft = new SelectList(_context.Aircraft.AsNoTracking().ToList(), "AircraftID", "TailNumber", selectedAircraft);
        }

        // GET: Lessons
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? instructorId, int? courseId, string status, string sortBy)
        {
            ViewBag.Instructors = new SelectList(_context.Instructors.AsNoTracking().ToList(), "InstructorID", "FullName");
            ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name");
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["Status"] = status;
            ViewData["SortBy"] = sortBy;

            var query = _context.Lessons
                .Include(l => l.Student)
                .Include(l => l.Instructor)
                .Include(l => l.Course)
                .Include(l => l.Aircraft)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(l => l.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(l => l.Date <= endDate.Value);
            if (instructorId.HasValue)
                query = query.Where(l => l.InstructorID == instructorId.Value);
            if (courseId.HasValue)
                query = query.Where(l => l.CourseID == courseId.Value);
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(l => l.Status == status);

            query = sortBy switch
            {
                "date_desc" => query.OrderByDescending(l => l.Date),
                "duration" => query.OrderBy(l => l.DurationHours),
                "duration_desc" => query.OrderByDescending(l => l.DurationHours),
                _ => query.OrderBy(l => l.Date)
            };

            var lessons = await query.AsNoTracking().ToListAsync();
            return View(lessons);
        }

        // GET: Lessons/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var lesson = await _context.Lessons
                .Include(l => l.Student)
                .Include(l => l.Instructor)
                .Include(l => l.Course)
                .Include(l => l.Aircraft)
                .Include(l => l.StatusChanges)
                .FirstOrDefaultAsync(l => l.LessonID == id);
            if (lesson == null) return NotFound();
            return View(lesson);
        }

        // GET: Lessons/Create
        public IActionResult Create(int? courseId)
        {
            PopulateLookups();
            var model = new Lesson { Date = DateTime.Today, Status = "Planned" };
            
            // Если передан courseId, автоматически подставляем его
            if (courseId.HasValue && courseId.Value > 0)
            {
                model.CourseID = courseId.Value;
                var course = _context.Courses.AsNoTracking().FirstOrDefault(c => c.CourseID == courseId.Value);
                if (course != null)
                {
                    ViewBag.CourseName = course.Name;
                    ViewBag.CourseId = courseId.Value;
                }
            }
            
            return View(model);
        }

        // POST: Lessons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lesson lesson)
        {
            // Очищаем все ошибки привязки модели для полей, которые мы обработаем вручную
            ModelState.Remove("StudentID");
            ModelState.Remove("InstructorID");
            ModelState.Remove("CourseID");
            ModelState.Remove("AircraftID");
            ModelState.Remove("Status");
            ModelState.Remove("Student");
            ModelState.Remove("Instructor");
            ModelState.Remove("Course");
            ModelState.Remove("Aircraft");
            
            // Обработка значений из формы
            var studentIdValue = Request.Form["StudentID"].ToString();
            var instructorIdValue = Request.Form["InstructorID"].ToString();
            
            if (string.IsNullOrEmpty(studentIdValue) || !int.TryParse(studentIdValue, out int studentId) || studentId <= 0)
            {
                ModelState.AddModelError("StudentID", "Выберите курсанта");
            }
            else
            {
                lesson.StudentID = studentId;
            }
            
            if (string.IsNullOrEmpty(instructorIdValue) || !int.TryParse(instructorIdValue, out int instructorId) || instructorId <= 0)
            {
                ModelState.AddModelError("InstructorID", "Выберите инструктора");
            }
            else
            {
                lesson.InstructorID = instructorId;
            }
            
            if (!string.IsNullOrEmpty(Request.Form["CourseID"]) && int.TryParse(Request.Form["CourseID"], out int courseId) && courseId > 0)
            {
                lesson.CourseID = courseId;
            }
            
            if (!string.IsNullOrEmpty(Request.Form["AircraftID"]) && int.TryParse(Request.Form["AircraftID"], out int aircraftId) && aircraftId > 0)
            {
                lesson.AircraftID = aircraftId;
            }
            
            // Обработка Status из формы
            var statusValue = Request.Form["Status"].ToString();
            if (!string.IsNullOrWhiteSpace(statusValue))
            {
                lesson.Status = statusValue;
            }
            else if (string.IsNullOrWhiteSpace(lesson.Status))
            {
                lesson.Status = "Planned";
            }
            
            // Валидация обязательных полей
            if (string.IsNullOrWhiteSpace(lesson.Status))
            {
                ModelState.AddModelError("Status", "Выберите статус");
            }
            
            if (lesson.DurationHours <= 0)
                ModelState.AddModelError("DurationHours", "Укажите длительность занятия больше 0");

            if (!ModelState.IsValid)
            {
                PopulateLookups(lesson.StudentID, lesson.InstructorID, lesson.CourseID, lesson.AircraftID);
                TempData["Error"] = "Проверьте правильность заполнения всех обязательных полей.";
                return View(lesson);
            }

            if (lesson.Date == default)
                lesson.Date = DateTime.Today;

            try
            {
                _context.Add(lesson);
                await _context.SaveChangesAsync();

                // Создаем запись об изменении статуса только если LessonID установлен
                if (lesson.LessonID > 0 && !string.IsNullOrWhiteSpace(lesson.Status))
                {
                    _context.LessonStatusChanges.Add(new LessonStatusChange
                    {
                        LessonID = lesson.LessonID,
                        OldStatus = "", // Пустая строка вместо null, так как в БД столбец не допускает NULL
                        NewStatus = lesson.Status,
                        ChangedAt = DateTime.UtcNow,
                        Comment = "Создано"
                    });
                    await _context.SaveChangesAsync();
                }
                
                TempData["Success"] = "Занятие успешно добавлено.";
                
                // Если курс был указан, перенаправляем на страницу деталей курса
                if (lesson.CourseID.HasValue && lesson.CourseID.Value > 0)
                {
                    return RedirectToAction("Details", "Courses", new { id = lesson.CourseID.Value });
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = $"Ошибка сохранения занятия: {ex.InnerException?.Message ?? ex.Message}";
                PopulateLookups(lesson.StudentID, lesson.InstructorID, lesson.CourseID, lesson.AircraftID);
                return View(lesson);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка: {ex.Message}";
                PopulateLookups(lesson.StudentID, lesson.InstructorID, lesson.CourseID, lesson.AircraftID);
                return View(lesson);
            }
        }

        // GET: Lessons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null) return NotFound();
            PopulateLookups(lesson.StudentID, lesson.InstructorID, lesson.CourseID, lesson.AircraftID);
            return View(lesson);
        }

        // POST: Lessons/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LessonID,StudentID,InstructorID,CourseID,AircraftID,Date,DurationHours,Topic,Status,Remarks")] Lesson lesson)
        {
            if (id != lesson.LessonID) return NotFound();
            if (lesson.StudentID <= 0)
                ModelState.AddModelError("StudentID", "Выберите курсанта");
            if (lesson.InstructorID <= 0)
                ModelState.AddModelError("InstructorID", "Выберите инструктора");
            if (lesson.DurationHours <= 0)
                ModelState.AddModelError("DurationHours", "Укажите длительность занятия больше 0");

            if (!ModelState.IsValid)
            {
                PopulateLookups(lesson.StudentID, lesson.InstructorID, lesson.CourseID, lesson.AircraftID);
                return View(lesson);
            }

            var existing = await _context.Lessons.FindAsync(id);
            if (existing == null) return NotFound();

            var oldStatus = existing.Status;
            existing.StudentID = lesson.StudentID;
            existing.InstructorID = lesson.InstructorID;
            existing.CourseID = lesson.CourseID;
            existing.AircraftID = lesson.AircraftID;
            existing.Date = lesson.Date;
            existing.DurationHours = lesson.DurationHours;
            existing.Topic = lesson.Topic;
            existing.Status = string.IsNullOrWhiteSpace(lesson.Status) ? existing.Status : lesson.Status;
            existing.Remarks = lesson.Remarks;

            try
            {
                await _context.SaveChangesAsync();

                if (!string.Equals(oldStatus, existing.Status, StringComparison.Ordinal))
                {
                    _context.LessonStatusChanges.Add(new LessonStatusChange
                    {
                        LessonID = existing.LessonID,
                        OldStatus = oldStatus,
                        NewStatus = existing.Status,
                        ChangedAt = DateTime.UtcNow,
                        Comment = "Обновление статуса"
                    });
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Ошибка сохранения занятия. Проверьте введенные данные.");
                PopulateLookups(lesson.StudentID, lesson.InstructorID, lesson.CourseID, lesson.AircraftID);
                return View(lesson);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Lessons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var lesson = await _context.Lessons
                .Include(l => l.Student)
                .Include(l => l.Instructor)
                .FirstOrDefaultAsync(m => m.LessonID == id);
            if (lesson == null) return NotFound();
            return View(lesson);
        }

        // POST: Lessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson != null)
            {
                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}



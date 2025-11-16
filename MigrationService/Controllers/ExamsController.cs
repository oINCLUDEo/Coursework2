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
    public class ExamsController : Controller
    {
        private readonly FlightSchoolDbContext _context;

        public ExamsController(FlightSchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int? studentFilter, int? courseFilter, string resultFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["ResultFilter"] = resultFilter;
            ViewBag.Students = new SelectList(_context.Students.AsNoTracking().ToList(), "StudentID", "FullName");
            ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name");

            var query = _context.Exams
                .Include(e => e.Student)
                .Include(e => e.Course)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var lowered = searchString.ToLower();
                query = query.Where(e =>
                    (e.Examiner != null && e.Examiner.ToLower().Contains(lowered)) ||
                    (e.Result != null && e.Result.ToLower().Contains(lowered)) ||
                    (e.Student != null && e.Student.FullName != null && e.Student.FullName.ToLower().Contains(lowered)) ||
                    (e.Course != null && e.Course.Name != null && e.Course.Name.ToLower().Contains(lowered))
                );
            }

            if (studentFilter.HasValue)
            {
                query = query.Where(e => e.StudentID == studentFilter.Value);
            }

            if (courseFilter.HasValue)
            {
                query = query.Where(e => e.CourseID == courseFilter.Value);
            }

            if (!string.IsNullOrWhiteSpace(resultFilter))
            {
                query = query.Where(e => e.Result == resultFilter);
            }

            var exams = await query
                .OrderByDescending(e => e.Date)
                .AsNoTracking()
                .ToListAsync();
            return View(exams);
        }

        public IActionResult Create(int? courseId)
        {
            ViewBag.Students = new SelectList(_context.Students.AsNoTracking().ToList(), "StudentID", "FullName");
            ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name");
            ViewBag.Instructors = _context.Instructors.AsNoTracking().ToList();
            
            var exam = new Exam();
            
            // Если передан courseId, автоматически подставляем его
            if (courseId.HasValue && courseId.Value > 0)
            {
                exam.CourseID = courseId.Value;
                var course = _context.Courses.AsNoTracking().FirstOrDefault(c => c.CourseID == courseId.Value);
                if (course != null)
                {
                    ViewBag.CourseName = course.Name;
                    ViewBag.CourseId = courseId.Value;
                }
            }
            
            return View(exam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Exam exam)
        {
            // Очищаем все ошибки привязки модели для полей, которые мы обработаем вручную
            ModelState.Remove("StudentID");
            ModelState.Remove("CourseID");
            ModelState.Remove("Student");
            ModelState.Remove("Course");
            
            // Обработка значений из формы
            var studentIdValue = Request.Form["StudentID"].ToString();
            var courseIdValue = Request.Form["CourseID"].ToString();
            
            if (string.IsNullOrEmpty(studentIdValue) || !int.TryParse(studentIdValue, out int studentId) || studentId <= 0)
            {
                ModelState.AddModelError("StudentID", "Выберите курсанта");
            }
            else
            {
                exam.StudentID = studentId;
            }
            
            if (string.IsNullOrEmpty(courseIdValue) || !int.TryParse(courseIdValue, out int courseId) || courseId <= 0)
            {
                ModelState.AddModelError("CourseID", "Выберите курс");
            }
            else
            {
                exam.CourseID = courseId;
            }
            
            if (exam.Date == default(DateTime))
                ModelState.AddModelError("Date", "Укажите дату экзамена");
            
            if (!ModelState.IsValid)
            {
                ViewBag.Students = new SelectList(_context.Students.AsNoTracking().ToList(), "StudentID", "FullName", exam.StudentID);
                ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name", exam.CourseID);
                ViewBag.Instructors = _context.Instructors.AsNoTracking().ToList();
                TempData["Error"] = "Проверьте правильность заполнения всех обязательных полей.";
                return View(exam);
            }
            
            try
            {
                _context.Add(exam);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Экзамен успешно добавлен.";
                
                // Если курс был указан, перенаправляем на страницу деталей курса
                if (exam.CourseID > 0)
                {
                    return RedirectToAction("Details", "Courses", new { id = exam.CourseID });
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "Не удалось сохранить экзамен. Проверьте введенные данные.";
                ViewBag.Students = new SelectList(_context.Students.AsNoTracking().ToList(), "StudentID", "FullName", exam.StudentID);
                ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name", exam.CourseID);
                ViewBag.Instructors = _context.Instructors.AsNoTracking().ToList();
                return View(exam);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null) return NotFound();
            ViewBag.Students = new SelectList(_context.Students.AsNoTracking().ToList(), "StudentID", "FullName", exam.StudentID);
            ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name", exam.CourseID);
            ViewBag.Instructors = _context.Instructors.AsNoTracking().ToList();
            return View(exam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ExamID,StudentID,CourseID,Date,Score,Result,Examiner")] Exam exam)
        {
            if (id != exam.ExamID) return NotFound();
            if (!ModelState.IsValid)
            {
                ViewBag.Students = new SelectList(_context.Students.AsNoTracking().ToList(), "StudentID", "FullName", exam.StudentID);
                ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name", exam.CourseID);
                ViewBag.Instructors = _context.Instructors.AsNoTracking().ToList();
                return View(exam);
            }
            var existing = await _context.Exams.FindAsync(id);
            if (existing == null) return NotFound();
            existing.StudentID = exam.StudentID;
            existing.CourseID = exam.CourseID;
            existing.Date = exam.Date;
            existing.Score = exam.Score;
            existing.Result = exam.Result;
            existing.Examiner = exam.Examiner;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Не удалось сохранить изменения. Проверьте введенные данные.";
                ViewBag.Students = new SelectList(_context.Students.AsNoTracking().ToList(), "StudentID", "FullName", exam.StudentID);
                ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name", exam.CourseID);
                ViewBag.Instructors = _context.Instructors.AsNoTracking().ToList();
                return View(exam);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var exam = await _context.Exams
                .Include(e => e.Student)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.ExamID == id);
            if (exam == null) return NotFound();
            return View(exam);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var exam = await _context.Exams
                .Include(e => e.Student)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.ExamID == id);
            if (exam == null) return NotFound();
            return View(exam);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam != null)
            {
                _context.Exams.Remove(exam);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}




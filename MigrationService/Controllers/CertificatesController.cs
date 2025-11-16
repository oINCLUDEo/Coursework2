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
    public class CertificatesController : Controller
    {
        private readonly FlightSchoolDbContext _context;

        public CertificatesController(FlightSchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int? studentFilter, int? courseFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewBag.Students = new SelectList(_context.Students.AsNoTracking().ToList(), "StudentID", "FullName");
            ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name");

            var query = _context.Certificates
                .Include(c => c.Student)
                .Include(c => c.Course)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var lowered = searchString.ToLower();
                query = query.Where(c =>
                    (c.CertificateNumber != null && c.CertificateNumber.ToLower().Contains(lowered)) ||
                    (c.Student != null && c.Student.FullName != null && c.Student.FullName.ToLower().Contains(lowered)) ||
                    (c.Course != null && c.Course.Name != null && c.Course.Name.ToLower().Contains(lowered))
                );
            }

            if (studentFilter.HasValue)
            {
                query = query.Where(c => c.StudentID == studentFilter.Value);
            }

            if (courseFilter.HasValue)
            {
                query = query.Where(c => c.CourseID == courseFilter.Value);
            }

            var list = await query
                .OrderByDescending(c => c.IssuedDate)
                .AsNoTracking()
                .ToListAsync();
            return View(list);
        }

        public IActionResult Create(int? courseId)
        {
            ViewBag.Students = new SelectList(_context.Students.AsNoTracking().ToList(), "StudentID", "FullName");
            ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name");
            
            var certificate = new Certificate();
            
            // Если передан courseId, автоматически подставляем его
            if (courseId.HasValue && courseId.Value > 0)
            {
                certificate.CourseID = courseId.Value;
                var course = _context.Courses.AsNoTracking().FirstOrDefault(c => c.CourseID == courseId.Value);
                if (course != null)
                {
                    ViewBag.CourseName = course.Name;
                    ViewBag.CourseId = courseId.Value;
                }
            }
            
            return View(certificate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Certificate certificate)
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
                certificate.StudentID = studentId;
            }
            
            if (string.IsNullOrEmpty(courseIdValue) || !int.TryParse(courseIdValue, out int courseId) || courseId <= 0)
            {
                ModelState.AddModelError("CourseID", "Выберите курс");
            }
            else
            {
                certificate.CourseID = courseId;
            }
            
            if (certificate.IssuedDate == default(DateTime))
                ModelState.AddModelError("IssuedDate", "Укажите дату выдачи");
            
            if (!ModelState.IsValid)
            {
                ViewBag.Students = new SelectList(_context.Students.AsNoTracking().ToList(), "StudentID", "FullName", certificate.StudentID);
                ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name", certificate.CourseID);
                TempData["Error"] = "Проверьте правильность заполнения всех обязательных полей.";
                return View(certificate);
            }
            
            try
            {
                _context.Add(certificate);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Сертификат успешно добавлен.";
                
                // Если курс был указан, перенаправляем на страницу деталей курса
                if (certificate.CourseID > 0)
                {
                    return RedirectToAction("Details", "Courses", new { id = certificate.CourseID });
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "Не удалось сохранить сертификат. Проверьте введенные данные.";
                ViewBag.Students = new SelectList(_context.Students.AsNoTracking().ToList(), "StudentID", "FullName", certificate.StudentID);
                ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name", certificate.CourseID);
                return View(certificate);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate == null) return NotFound();
            ViewBag.Students = new SelectList(_context.Students.AsNoTracking().ToList(), "StudentID", "FullName", certificate.StudentID);
            ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name", certificate.CourseID);
            return View(certificate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CertificateID,StudentID,CourseID,IssuedDate,CertificateNumber,ValidUntil")] Certificate certificate)
        {
            if (id != certificate.CertificateID) return NotFound();
            if (!ModelState.IsValid)
            {
                ViewBag.Students = new SelectList(_context.Students.AsNoTracking().ToList(), "StudentID", "FullName", certificate.StudentID);
                ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name", certificate.CourseID);
                return View(certificate);
            }
            var existing = await _context.Certificates.FindAsync(id);
            if (existing == null) return NotFound();
            existing.StudentID = certificate.StudentID;
            existing.CourseID = certificate.CourseID;
            existing.IssuedDate = certificate.IssuedDate;
            existing.CertificateNumber = certificate.CertificateNumber;
            existing.ValidUntil = certificate.ValidUntil;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Не удалось сохранить изменения. Проверьте введенные данные.";
                ViewBag.Students = new SelectList(_context.Students.AsNoTracking().ToList(), "StudentID", "FullName", certificate.StudentID);
                ViewBag.Courses = new SelectList(_context.Courses.AsNoTracking().ToList(), "CourseID", "Name", certificate.CourseID);
                return View(certificate);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var certificate = await _context.Certificates
                .Include(c => c.Student)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.CertificateID == id);
            if (certificate == null) return NotFound();
            return View(certificate);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var certificate = await _context.Certificates
                .Include(c => c.Student)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.CertificateID == id);
            if (certificate == null) return NotFound();
            return View(certificate);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate != null)
            {
                _context.Certificates.Remove(certificate);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}




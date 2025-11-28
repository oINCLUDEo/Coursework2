using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigrationService.Models;

namespace MigrationService.Controllers
{
    public class AuthController : Controller
    {
        private readonly FlightSchoolDbContext _context;

        private const string SessionKeyUser = "AuthUser";
        private const string SessionKeyOtp = "AuthOtp";
        private const string SessionKeyOtpExpiry = "AuthOtpExpiry";

        public AuthController(FlightSchoolDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            var cookie = HttpContext.Request.Cookies["FS-Auth"];
            if (!string.IsNullOrEmpty(cookie))
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            var email = request.Email?.Trim().ToLower();
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(nameof(LoginRequest.Email), "Введите email.");
                return View(request);
            }
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email != null && s.Email.ToLower() == email);
            var instructor = student == null ? await _context.Instructors.FirstOrDefaultAsync(i => i.Email != null && i.Email.ToLower() == email) : null;
            if (student == null && instructor == null)
            {
                ModelState.AddModelError(string.Empty, "Пользователь с таким email не найден.");
                return View(request);
            }

            var otp = GenerateOtp();
            HttpContext.Session.SetString(SessionKeyOtp, Hash(otp));
            HttpContext.Session.SetString(SessionKeyOtpExpiry, DateTime.UtcNow.AddMinutes(5).ToString("O"));
            HttpContext.Session.SetString(SessionKeyUser, email);
            
            // Сохраняем OTP в сессии для отображения в Development режиме
            var isDevelopment = HttpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() ?? false;
            if (isDevelopment)
            {
                HttpContext.Session.SetString("DevOtp", otp);
            }

            return RedirectToAction("Verify");
        }

        public IActionResult Verify()
        {
            // Проверяем, есть ли данные в сессии
            var storedHash = HttpContext.Session.GetString(SessionKeyOtp);
            var email = HttpContext.Session.GetString(SessionKeyUser);
            
            if (storedHash == null || email == null)
            {
                // Если сессия пуста, перенаправляем на страницу входа
                TempData["Error"] = "Сеанс истек. Пожалуйста, войдите снова.";
                return RedirectToAction("Login");
            }
            
            // В Development режиме показываем OTP
            var isDevelopment = HttpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() ?? false;
            if (isDevelopment)
            {
                var devOtp = HttpContext.Session.GetString("DevOtp");
                if (!string.IsNullOrEmpty(devOtp))
                {
                    ViewBag.DevOtp = devOtp;
                }
            }
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Verify(string code)
        {
            var storedHash = HttpContext.Session.GetString(SessionKeyOtp);
            var expiryStr = HttpContext.Session.GetString(SessionKeyOtpExpiry);
            var email = HttpContext.Session.GetString(SessionKeyUser);

            if (storedHash == null || expiryStr == null || email == null)
            {
                TempData["Error"] = "Сеанс истек. Попробуйте снова.";
                return RedirectToAction("Login");
            }

            if (DateTime.TryParse(expiryStr, out var expiry) && DateTime.UtcNow > expiry)
            {
                TempData["Error"] = "Код истек. Запросите новый.";
                HttpContext.Session.Remove(SessionKeyOtp);
                HttpContext.Session.Remove(SessionKeyOtpExpiry);
                HttpContext.Session.Remove(SessionKeyUser);
                HttpContext.Session.Remove("DevOtp");
                return RedirectToAction("Login");
            }

            if (string.IsNullOrWhiteSpace(code) || !string.Equals(storedHash, Hash(code), StringComparison.Ordinal))
            {
                ModelState.AddModelError("code", "Неверный код. Попробуйте снова.");
                var isDev = HttpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() ?? false;
                if (isDev)
                {
                    var devOtp = HttpContext.Session.GetString("DevOtp");
                    if (!string.IsNullOrEmpty(devOtp))
                    {
                        ViewBag.DevOtp = devOtp;
                    }
                }
                return View();
            }

            // Очищаем сессию после успешной верификации
            HttpContext.Session.Remove(SessionKeyOtp);
            HttpContext.Session.Remove(SessionKeyOtpExpiry);
            HttpContext.Session.Remove(SessionKeyUser);
            HttpContext.Session.Remove("DevOtp");

            var isDevelopment = HttpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() ?? false;
            Response.Cookies.Append("FS-Auth", email, new CookieOptions
            {
                HttpOnly = true,
                Secure = !isDevelopment,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(8)
            });

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("FS-Auth");
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private static string GenerateOtp()
        {
            var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var value = BitConverter.ToUInt32(bytes, 0) % 1000000;
            return value.ToString("D6");
        }

        private static string Hash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input ?? string.Empty));
            return Convert.ToHexString(bytes);
        }

        public class LoginRequest
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;
        }
    }
}

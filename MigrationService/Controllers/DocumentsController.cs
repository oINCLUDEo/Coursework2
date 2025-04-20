using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigrationService.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace MigrationService.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly MigrationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DocumentsController(MigrationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Documents
        public async Task<IActionResult> Index()
        {
            var documents = await _context.Documents
                .Include(d => d.Application)
                    .ThenInclude(a => a.Migrant)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
            return View(documents);
        }

        // GET: Documents/Upload/5
        public async Task<IActionResult> Upload(int? id)
        {
            if (id == null)
                return NotFound();

            var application = await _context.Applications
                .Include(a => a.Migrant)
                .FirstOrDefaultAsync(a => a.ApplicationID == id);

            if (application == null)
                return NotFound();

            ViewBag.ApplicationID = id;
            ViewBag.MigrantName = application.Migrant?.FullName;
            return View();
        }

        // POST: Documents/Upload/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file to upload");
                return View();
            }

            var application = await _context.Applications.FindAsync(id);
            if (application == null)
                return NotFound();

            // Create uploads directory if it doesn't exist
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Create document record
            var document = new Document
            {
                ApplicationID = id,
                FileName = file.FileName,
                FileType = Path.GetExtension(file.FileName),
                UploadedAt = DateTime.Now
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            // Update the document with the correct file path
            document.FileName = uniqueFileName;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Applications", new { id = id });
        }

        // GET: Documents/Download/5
        public async Task<IActionResult> Download(int? id)
        {
            if (id == null)
                return NotFound();

            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return NotFound();

            var filePath = Path.Combine(_environment.WebRootPath, "uploads", document.FileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, GetContentType(document.FileType), document.FileName.Replace($"{document.DocumentID}_", ""));
        }

        // POST: Documents/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return NotFound();

            var filePath = Path.Combine(_environment.WebRootPath, "uploads", document.FileName);

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            var applicationId = document.ApplicationID;
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Applications", new { id = applicationId });
        }

        private string GetContentType(string fileType)
        {
            return fileType.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }
    }
} 
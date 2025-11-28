using Microsoft.EntityFrameworkCore;
using MigrationService.Controllers;

namespace MigrationService.Models
{
    public class FlightSchoolDbContext : DbContext
    {
        public FlightSchoolDbContext(DbContextOptions<FlightSchoolDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Aircraft> Aircraft { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonStatusChange> LessonStatusChanges { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<StudentCertificate> StudentCertificates { get; set; }
        public DbSet<HomeController.UpcomingLessonResult> UpcomingLessonResults { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>().ToTable("Students");
            modelBuilder.Entity<Instructor>().ToTable("Instructors");
            modelBuilder.Entity<Aircraft>().ToTable("Aircraft");
            modelBuilder.Entity<Course>().ToTable("Courses");
            modelBuilder.Entity<Lesson>().ToTable("Lessons");
            modelBuilder.Entity<LessonStatusChange>().ToTable("LessonStatusChanges");
            modelBuilder.Entity<Exam>().ToTable("Exams");
            modelBuilder.Entity<Certificate>().ToTable("Certificates");
            modelBuilder.Entity<StudentCertificate>().ToTable("StudentCertificates");

            modelBuilder.Entity<Student>()
                .HasOne(s => s.Course)
                .WithMany(c => c.Students)
                .HasForeignKey(s => s.CourseID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Student)
                .WithMany(s => s.Lessons)
                .HasForeignKey(l => l.StudentID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Instructor)
                .WithMany(i => i.Lessons)
                .HasForeignKey(l => l.InstructorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Course)
                .WithMany()
                .HasForeignKey(l => l.CourseID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Aircraft)
                .WithMany()
                .HasForeignKey(l => l.AircraftID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<LessonStatusChange>()
                .HasOne(lc => lc.Lesson)
                .WithMany(l => l.StatusChanges)
                .HasForeignKey(lc => lc.LessonID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Exam>()
                .HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Exam>()
                .HasOne(e => e.Course)
                .WithMany()
                .HasForeignKey(e => e.CourseID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Exam>()
                .HasOne(e => e.Instructor)
                .WithMany()
                .HasForeignKey(e => e.InstructorID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.Course)
                .WithMany()
                .HasForeignKey(c => c.CourseID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Certificate>()
                .HasMany(c => c.StudentCertificates)
                .WithOne(sc => sc.Certificate)
                .HasForeignKey(sc => sc.CertificateID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Student>()
                .HasMany(s => s.StudentCertificates)
                .WithOne(sc => sc.Student)
                .HasForeignKey(sc => sc.StudentID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentCertificate>()
                .Property(sc => sc.Status)
                .HasDefaultValue("Активен");

            modelBuilder.Entity<Instructor>()
                .Property(i => i.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<Course>()
                .Property(c => c.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<Lesson>()
                .Property(l => l.Status)
                .HasDefaultValue("Запланировано");

            modelBuilder.Entity<LessonStatusChange>()
                .Property(lsc => lsc.ChangedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
        
        public async Task<decimal> GetTotalFlightHoursByStudentAsync(int studentId)
        {
            var result = await Database.SqlQueryRaw<decimal>(
                    "SELECT dbo.fn_TotalFlightHoursByStudent({0})", studentId)
                .ToListAsync();
        
            return result.First();
        }
        
        public async Task<List<HomeController.UpcomingLessonResult>> GetUpcomingLessonsAsync()
        {
            return await UpcomingLessonResults
                .FromSqlRaw("EXEC dbo.sp_GetUpcomingLessons")
                .AsNoTracking()
                .ToListAsync();
        }
    }
}

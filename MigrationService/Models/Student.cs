using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MigrationService.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class Student
    {
        public int StudentID { get; set; }

        [Required]
        [Display(Name = "ФИО")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Адрес")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Дата рождения")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Медицинский сертификат №")]
        public string MedicalCertificateNo { get; set; } = string.Empty;

        [Display(Name = "Дата зачисления")]
        [DataType(DataType.Date)]
        public DateTime? EnrollmentDate { get; set; }

        [Display(Name = "Заметки")]
        public string Notes { get; set; } = string.Empty;

        [Display(Name = "Курс")]
        public int? CourseID { get; set; }

        public virtual Course? Course { get; set; }
        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public virtual ICollection<StudentCertificate> StudentCertificates { get; set; } = new List<StudentCertificate>();
    }
}

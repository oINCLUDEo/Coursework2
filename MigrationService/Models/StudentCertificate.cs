using System;
using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models
{
    public class StudentCertificate
    {
        public int StudentCertificateID { get; set; }

        [Required]
        [Display(Name = "Курсант")]
        public int StudentID { get; set; }

        [Required]
        [Display(Name = "Сертификат")]
        public int CertificateID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Дата выдачи")]
        public DateTime IssuedDate { get; set; } = DateTime.Today;

        [Display(Name = "Номер сертификата")]
        public string CertificateNumber { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Действителен до")]
        public DateTime? ValidUntil { get; set; }

        [Display(Name = "Статус")]
        public string Status { get; set; } = "Активен";

        [Display(Name = "Комментарий")]
        public string? Notes { get; set; }

        public virtual Student Student { get; set; } = null!;
        public virtual Certificate Certificate { get; set; } = null!;
    }
}


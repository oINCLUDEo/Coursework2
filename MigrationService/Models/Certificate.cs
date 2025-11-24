using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MigrationService.Models
{
    public class Certificate
    {
        public int CertificateID { get; set; }

        [Required]
        [Display(Name = "Название сертификата")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Описание")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Привязанный курс")]
        public int? CourseID { get; set; }

        [Display(Name = "Базовый срок действия (дни)")]
        public int? DefaultValidityDays { get; set; }

        public virtual Course? Course { get; set; }

        public virtual ICollection<StudentCertificate> StudentCertificates { get; set; } = new List<StudentCertificate>();
    }
}



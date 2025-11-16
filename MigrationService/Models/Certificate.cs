using System;
using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models
{
    public class Certificate
    {
        public int CertificateID { get; set; }

        [Required]
        public int StudentID { get; set; }
        
        [Required]
        public int CourseID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Дата выдачи")]
        public DateTime IssuedDate { get; set; }

        [Display(Name = "Номер сертификата")]
        public string CertificateNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Действителен до")]
        public DateTime? ValidUntil { get; set; }

        public virtual Student Student { get; set; }
        public virtual Course Course { get; set; }
    }
}



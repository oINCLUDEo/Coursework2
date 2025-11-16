using System;
using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models
{
    public class StudentCourse
    {
        public int StudentID { get; set; }
        public int CourseID { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Дата зачисления")]
        public DateTime EnrolledAt { get; set; }

        [Display(Name = "Прогресс (часы)")]
        public decimal ProgressHours { get; set; }

        [Display(Name = "Статус")]
        public string Status { get; set; }

        public virtual Student Student { get; set; }
        public virtual Course Course { get; set; }
    }
}



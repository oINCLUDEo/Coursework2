using System;
using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models
{
    public class Exam
    {
        public int ExamID { get; set; }

        [Required]
        public int StudentID { get; set; }
        
        [Required]
        public int CourseID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Дата экзамена")]
        public DateTime Date { get; set; }

        [Display(Name = "Оценка")]
        public decimal? Score { get; set; }

        [Display(Name = "Результат")]
        public string Result { get; set; } = string.Empty;

        [Display(Name = "Экзаменатор")]
        public int? InstructorID { get; set; }

        public virtual Student Student { get; set; } = null!;
        public virtual Course Course { get; set; } = null!;
        public virtual Instructor? Instructor { get; set; }
    }
}



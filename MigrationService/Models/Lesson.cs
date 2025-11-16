using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models
{
    public class Lesson
    {
        public int LessonID { get; set; }

        [Required]
        [Display(Name = "Курсант")]
        public int StudentID { get; set; }

        [Required]
        [Display(Name = "Инструктор")]
        public int InstructorID { get; set; }

        [Display(Name = "Курс")]
        public int? CourseID { get; set; }

        [Display(Name = "Самолет")]
        public int? AircraftID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Дата занятия")]
        public DateTime Date { get; set; }

        [Required]
        [Range(0.1, 1000)]
        [Display(Name = "Длительность (часы)")]
        public decimal DurationHours { get; set; }

        [Display(Name = "Тема")]
        public string Topic { get; set; }

        [Required]
        [Display(Name = "Статус")]
        public string Status { get; set; } = "Planned";

        [Display(Name = "Примечания")]
        public string Remarks { get; set; }

        public virtual Student Student { get; set; }
        public virtual Instructor Instructor { get; set; }
        public virtual Course Course { get; set; }
        public virtual Aircraft Aircraft { get; set; }
        public virtual ICollection<LessonStatusChange> StatusChanges { get; set; } = new List<LessonStatusChange>();
    }
}



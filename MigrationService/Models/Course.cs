using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models
{
    public class Course
    {
        public int CourseID { get; set; }

        [Required]
        [Display(Name = "Название курса")]
        public string Name { get; set; }

        [Display(Name = "Категория")]
        public string Category { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Требуемые часы")]
        public decimal? RequiredHours { get; set; }

        [Display(Name = "Активен")]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }
}



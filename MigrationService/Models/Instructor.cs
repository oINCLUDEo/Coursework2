using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MigrationService.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class Instructor
    {
        public int InstructorID { get; set; }

        [Required]
        [Display(Name = "ФИО")]
        public string FullName { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Телефон")]
        public string Phone { get; set; }

        [Display(Name = "Звание")]
        public string Rank { get; set; }

        [Display(Name = "Дата найма")]
        [DataType(DataType.Date)]
        public DateTime? HireDate { get; set; }

        [Display(Name = "Активен")]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}



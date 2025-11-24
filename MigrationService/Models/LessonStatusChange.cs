using System;
using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models
{
    public class LessonStatusChange
    {
        public int LessonStatusChangeID { get; set; }

        public int LessonID { get; set; }

        [Display(Name = "Когда изменен")]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Было")]
        public string OldStatus { get; set; } = string.Empty;

        [Display(Name = "Стало")]
        public string NewStatus { get; set; } = string.Empty;

        [Display(Name = "Комментарий")]
        public string Comment { get; set; } = string.Empty;

        public virtual Lesson Lesson { get; set; } = null!;
    }
}



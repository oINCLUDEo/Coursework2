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
        public string OldStatus { get; set; }

        [Display(Name = "Стало")]
        public string NewStatus { get; set; }

        [Display(Name = "Комментарий")]
        public string Comment { get; set; }

        public virtual Lesson Lesson { get; set; }
    }
}



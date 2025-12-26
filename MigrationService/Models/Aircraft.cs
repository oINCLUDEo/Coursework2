using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MigrationService.Models
{
    [Index(nameof(TailNumber), IsUnique = true)]
    public class Aircraft
    {
        public int AircraftID { get; set; }

        [Required]
        [Display(Name = "Бортовой номер")]
        public string TailNumber { get; set; } = string.Empty;

        [Display(Name = "Модель")]
        public string Model { get; set; } = string.Empty;

        [Display(Name = "Тип")]
        public string Type { get; set; } = string.Empty;

        [Display(Name = "Год выпуска")]
        public int? Year { get; set; }

        [Display(Name = "Налёт (часы)")]
        public decimal? TotalHours { get; set; }

        [Display(Name = "Статус")]
        public string Status { get; set; } = string.Empty;

        public virtual ICollection<MaintenanceLog> MaintenanceLogs { get; set; } = new List<MaintenanceLog>();
    }
}

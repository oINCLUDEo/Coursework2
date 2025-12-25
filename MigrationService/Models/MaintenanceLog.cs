using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models
{
    public class MaintenanceLog
    {
        public int MaintenanceLogID { get; set; }

        [Required]
        [Display(Name = "Самолет")]
        public int AircraftID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Дата обслуживания")]
        public DateTime MaintenanceDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Тип обслуживания")]
        public string MaintenanceType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Описание работ")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Стоимость")]
        public decimal? Cost { get; set; }

        [Required]
        [Display(Name = "Механик")]
        public string MechanicName { get; set; } = string.Empty;

        [Display(Name = "Примечания")]
        public string? Notes { get; set; }

        public virtual Aircraft Aircraft { get; set; } = null!;
    }
}

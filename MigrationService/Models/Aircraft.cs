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
        public string TailNumber { get; set; }

        [Display(Name = "Модель")]
        public string Model { get; set; }

        [Display(Name = "Тип")]
        public string Type { get; set; }

        [Display(Name = "Год выпуска")]
        public int? Year { get; set; }

        [Display(Name = "Налёт (часы)")]
        public decimal? TotalHours { get; set; }

        [Display(Name = "Статус")]
        public string Status { get; set; }
    }
}



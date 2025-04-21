using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MigrationService.Models;

public class Country
{
    public int CountryID { get; set; }

    [Required(ErrorMessage = "Название страны обязательно для заполнения")]
    [Display(Name = "Название страны")]
    public string CountryName { get; set; }

    [Required(ErrorMessage = "ISO код обязателен для заполнения")]
    [Display(Name = "ISO код")]
    public string ISOCode { get; set; }

    [Display(Name = "Требуется виза")]
    public bool VisaRequired { get; set; }

    [ValidateNever]
    public virtual ICollection<Migrant> Migrants { get; set; }
}

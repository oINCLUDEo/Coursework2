using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MigrationService.Models;

public class Country
{
    public int CountryID { get; set; }

    [Required(ErrorMessage = "Country name is required")]
    public string CountryName { get; set; }

    [Required(ErrorMessage = "ISO code is required")]
    public string ISOCode { get; set; }

    public bool VisaRequired { get; set; }

    [ValidateNever]
    public virtual ICollection<Migrant> Migrants { get; set; }
}

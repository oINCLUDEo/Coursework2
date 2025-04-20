using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MigrationService.Models;

public class Migrant
{
    public int MigrantID { get; set; }

    [Required]
    public string FullName { get; set; }

    [Required]
    public string PassportNumber { get; set; }

    public DateTime? BirthDate { get; set; }

    public string Address { get; set; }

    public int? CountryID { get; set; }

    [Required]
    [Display(Name = "Gender")]
    public string Gender { get; set; }

    [Required]
    [Display(Name = "Phone Number")]
    [Phone]
    public string PhoneNumber { get; set; }

    [ValidateNever]
    public virtual Country Country { get; set; }

    [ValidateNever]
    public virtual ICollection<Application> Applications { get; set; }

    [ValidateNever]
    public virtual ICollection<MigrantLanguage> MigrantLanguages { get; set; }
}


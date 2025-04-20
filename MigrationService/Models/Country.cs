using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models;

public class Country
{
    public int CountryID { get; set; }

    [Required]
    public string CountryName { get; set; }

    public string ISOCode { get; set; }

    public bool VisaRequired { get; set; }

    public virtual ICollection<Migrant> Migrants { get; set; }
}

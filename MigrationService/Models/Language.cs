using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models;

public class Language
{
    public int LanguageID { get; set; }

    [Required]
    public string LanguageName { get; set; }

    public virtual ICollection<MigrantLanguage> MigrantLanguages { get; set; }
}

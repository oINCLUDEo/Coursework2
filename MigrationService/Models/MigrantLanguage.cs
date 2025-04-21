using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MigrationService.Models;

public class MigrantLanguage
{
    [Key, Column(Order = 0)]
    [Display(Name = "Мигрант")]
    public int MigrantID { get; set; }

    [Key, Column(Order = 1)]
    [Display(Name = "Язык")]
    public int LanguageID { get; set; }

    [Display(Name = "Уровень владения")]
    public string ProficiencyLevel { get; set; }

    public virtual Migrant Migrant { get; set; }

    public virtual Language Language { get; set; }
}

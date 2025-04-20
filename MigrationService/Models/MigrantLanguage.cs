using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MigrationService.Models;

public class MigrantLanguage
{
    [Key, Column(Order = 0)]
    public int MigrantID { get; set; }

    [Key, Column(Order = 1)]
    public int LanguageID { get; set; }

    public string ProficiencyLevel { get; set; }

    public virtual Migrant Migrant { get; set; }

    public virtual Language Language { get; set; }
}

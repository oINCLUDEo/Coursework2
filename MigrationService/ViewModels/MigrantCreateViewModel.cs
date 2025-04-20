using Microsoft.AspNetCore.Mvc.ModelBinding;
using MigrationService.Models;

namespace MigrationService;

public class LanguageProficiencyEntry
{
    public int LanguageID { get; set; }
    public string LanguageName { get; set; }
    public string ProficiencyLevel { get; set; }
}

public class MigrantCreateViewModel
{
    public Migrant Migrant { get; set; }

    [BindNever]
    public List<Country> Countries { get; set; } = new();

    [BindNever]
    public List<Language> Languages { get; set; } = new();

    public List<LanguageProficiencyEntry> SelectedLanguageProficiencies { get; set; } = new();
}

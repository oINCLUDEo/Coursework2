using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MigrationService.Models;

public class Language
{
    public int LanguageID { get; set; }

    [Required(ErrorMessage = "Поле 'Название языка' обязательно для заполнения")]
    [Display(Name = "Название языка")]
    public string LanguageName { get; set; }

    [ValidateNever]
    public virtual ICollection<MigrantLanguage> MigrantLanguages { get; set; }
}

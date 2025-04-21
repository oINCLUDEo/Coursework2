using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MigrationService.Models;

public class Migrant
{
    public int MigrantID { get; set; }

    [Required(ErrorMessage = "ФИО обязательно для заполнения")]
    [Display(Name = "ФИО")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Номер паспорта обязателен для заполнения")]
    [Display(Name = "Номер паспорта")]
    public string PassportNumber { get; set; }

    [Display(Name = "Дата рождения")]
    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }

    [Display(Name = "Адрес")]
    public string Address { get; set; }

    [Display(Name = "Страна")]
    public int? CountryID { get; set; }

    [Required(ErrorMessage = "Пол обязателен для заполнения")]
    [Display(Name = "Пол")]
    public string Gender { get; set; }

    [Required(ErrorMessage = "Номер телефона обязателен для заполнения")]
    [Display(Name = "Номер телефона")]
    [Phone(ErrorMessage = "Некорректный формат номера телефона")]
    public string PhoneNumber { get; set; }

    [ValidateNever]
    public virtual Country Country { get; set; }

    [ValidateNever]
    public virtual ICollection<Application> Applications { get; set; }

    [ValidateNever]
    public virtual ICollection<MigrantLanguage> MigrantLanguages { get; set; }
}


using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace MigrationService.Models;

[Index(nameof(Officer.Login), IsUnique = true)]
public class Officer
{
    public int OfficerID { get; set; }

    [Required(ErrorMessage = "ФИО обязательно для заполнения")]
    [Display(Name = "ФИО")]
    public string FullName { get; set; }

    [Display(Name = "Должность")]
    public string Position { get; set; }

    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Логин обязателен для заполнения")]
    [Display(Name = "Логин")]
    public string Login { get; set; }

    [Required(ErrorMessage = "Пароль обязателен для заполнения")]
    [Display(Name = "Пароль")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [ValidateNever]
    public virtual ICollection<Application> Applications { get; set; }
}

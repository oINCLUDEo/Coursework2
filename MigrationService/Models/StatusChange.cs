using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MigrationService.Models;

public class StatusChange
{
    public int StatusChangeID { get; set; }

    [Required(ErrorMessage = "Необходимо указать заявление")]
    [Display(Name = "Заявление")]
    public int ApplicationID { get; set; }

    [Required(ErrorMessage = "Статус обязателен для заполнения")]
    [Display(Name = "Статус")]
    public string Status { get; set; }

    [Display(Name = "Дата изменения")]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
    public DateTime ChangedAt { get; set; }
    
    [ValidateNever]
    [Display(Name = "Комментарий")]
    public string Comment { get; set; }

    public virtual Application Application { get; set; }
}

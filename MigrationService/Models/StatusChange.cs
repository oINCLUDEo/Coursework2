using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models;

public class StatusChange
{
    public int StatusChangeID { get; set; }

    [Display(Name = "Заявление")]
    public int ApplicationID { get; set; }

    [Display(Name = "Статус")]
    public string Status { get; set; }

    [Display(Name = "Дата изменения")]
    [DataType(DataType.DateTime)]
    public DateTime ChangedAt { get; set; }
    
    [ValidateNever]
    [Display(Name = "Комментарий")]
    public string Comment { get; set; }

    public virtual Application Application { get; set; }
}

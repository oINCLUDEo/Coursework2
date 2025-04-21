using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MigrationService.Models;

public class Application
{
    public int ApplicationID { get; set; }

    [Required(ErrorMessage = "Необходимо выбрать мигранта")]
    [Display(Name = "Мигрант")]
    public int MigrantID { get; set; }

    [Required(ErrorMessage = "Необходимо выбрать сотрудника")]
    [Display(Name = "Сотрудник")]
    public int OfficerID { get; set; }

    [Required(ErrorMessage = "Тип заявления обязателен для заполнения")]
    [Display(Name = "Тип заявления")]
    public string Type { get; set; }

    [Required(ErrorMessage = "Статус обязателен для заполнения")]
    [Display(Name = "Статус")]
    public string Status { get; set; }

    [Display(Name = "Дата подачи")]
    [DataType(DataType.DateTime)]
    public DateTime SubmissionDate { get; set; }

    [Display(Name = "Дата решения")]
    [DataType(DataType.DateTime)]
    public DateTime? DecisionDate { get; set; }
    [ValidateNever]
    public virtual Migrant Migrant { get; set; }
    [ValidateNever]
    public virtual Officer Officer { get; set; }
    [ValidateNever]
    public virtual ICollection<Document> Documents { get; set; }
    [ValidateNever]
    public virtual ICollection<StatusChange> StatusChanges { get; set; }
}

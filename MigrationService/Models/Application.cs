using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models;

public class Application
{
    public int ApplicationID { get; set; }

    [Required(ErrorMessage = "Migrant is required")]
    [Display(Name = "Migrant")]
    public int MigrantID { get; set; }

    [Required(ErrorMessage = "Officer is required")]
    [Display(Name = "Officer")]
    public int OfficerID { get; set; }

    [Required(ErrorMessage = "Application type is required")]
    [Display(Name = "Application Type")]
    public string Type { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [Display(Name = "Status")]
    public string Status { get; set; }

    [Required(ErrorMessage = "Submission date is required")]
    [Display(Name = "Submission Date")]
    [DataType(DataType.DateTime)]
    public DateTime SubmissionDate { get; set; }

    [Display(Name = "Decision Date")]
    [DataType(DataType.DateTime)]
    public DateTime? DecisionDate { get; set; }

    public virtual Migrant Migrant { get; set; }

    public virtual Officer Officer { get; set; }

    public virtual ICollection<Document> Documents { get; set; }

    public virtual ICollection<StatusChange> StatusChanges { get; set; }
}

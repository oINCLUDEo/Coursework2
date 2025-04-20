using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MigrationService.Models;

public class StatusChange
{
    public int StatusChangeID { get; set; }

    public int ApplicationID { get; set; }

    public string Status { get; set; }

    public DateTime ChangedAt { get; set; }
    
    [ValidateNever]
    public string Comment { get; set; }

    public virtual Application Application { get; set; }
}

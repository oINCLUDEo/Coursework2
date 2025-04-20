using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MigrationService.Models;

[Index(nameof(Officer.Login), IsUnique = true)]
public class Officer
{
    public int OfficerID { get; set; }

    [Required]
    public string FullName { get; set; }

    public string Position { get; set; }

    public string Email { get; set; }

    [Required]
    public string Login { get; set; }

    [Required]
    public string Password { get; set; }

    public virtual ICollection<Application> Applications { get; set; }
}

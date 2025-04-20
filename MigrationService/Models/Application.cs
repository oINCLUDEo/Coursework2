namespace MigrationService.Models;

public class Application
{
    public int ApplicationID { get; set; }

    public int MigrantID { get; set; }

    public int OfficerID { get; set; }

    public string Type { get; set; }

    public string Status { get; set; }

    public DateTime? SubmissionDate { get; set; }

    public DateTime? DecisionDate { get; set; }

    public virtual Migrant Migrant { get; set; }

    public virtual Officer Officer { get; set; }

    public virtual ICollection<Document> Documents { get; set; }

    public virtual ICollection<StatusChange> StatusChanges { get; set; }
}

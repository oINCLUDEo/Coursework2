namespace MigrationService.Models;

public class Document
{
    public int DocumentID { get; set; }

    public int ApplicationID { get; set; }

    public string FileName { get; set; }

    public string FileType { get; set; }

    public DateTime UploadedAt { get; set; }

    public virtual Application Application { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models;

public class Document
{
    public int DocumentID { get; set; }

    [Display(Name = "Заявление")]
    public int ApplicationID { get; set; }

    [Display(Name = "Имя файла")]
    public string FileName { get; set; }

    [Display(Name = "Тип файла")]
    public string FileType { get; set; }

    [Display(Name = "Дата загрузки")]
    [DataType(DataType.DateTime)]
    public DateTime UploadedAt { get; set; }

    public virtual Application Application { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models;

public class Document
{
    public int DocumentID { get; set; }

    [Required(ErrorMessage = "Необходимо указать заявление")]
    [Display(Name = "Заявление")]
    public int ApplicationID { get; set; }

    [Required(ErrorMessage = "Имя файла обязательно для заполнения")]
    [Display(Name = "Имя файла")]
    public string FileName { get; set; }

    [Required(ErrorMessage = "Тип файла обязателен для заполнения")]
    [Display(Name = "Тип файла")]
    public string FileType { get; set; }

    [Display(Name = "Дата загрузки")]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
    public DateTime UploadedAt { get; set; }

    public virtual Application Application { get; set; }
}

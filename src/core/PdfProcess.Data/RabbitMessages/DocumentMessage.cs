namespace PdfProcess.Data.RabbitMessages;

public class DocumentMessage
{
    public Guid DocumentId { get; set; }
    public string FilePath { get; set; }
    public string FileName { get; set; }
}

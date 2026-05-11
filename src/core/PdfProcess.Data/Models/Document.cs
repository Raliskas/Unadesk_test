using PdfProcess.Data.Enums;

namespace PdfProcess.Data.Models;

public class Document
{
    public Guid Id { get; set; }

    public string FileName { get; set; }

    public string FilePath { get; set; }

    public long FileSize { get; set; }

    public string ContentType { get; set; }

    public string? ExtractedText { get; set; }

    public DocumentStatus Status { get; set; }

    //TODO It would be possible to additionally create a table in which the history of status changes would be stored.

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public string? ErrorMessage { get; set; }
}

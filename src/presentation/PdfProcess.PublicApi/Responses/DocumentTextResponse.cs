namespace PdfProcess.PublicApi.Responses;

public class DocumentTextResponse
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string ExtractedText { get; set; }

    public DocumentTextResponse(Guid id, string fileName, string extractedText)
    {
        Id = id;
        FileName = fileName;
        ExtractedText = extractedText;
    }
}
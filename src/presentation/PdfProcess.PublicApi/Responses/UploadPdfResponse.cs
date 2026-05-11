namespace PdfProcess.PublicApi.Responses;

public class UploadPdfResponse
{
    public Guid Id { get; set; }
    public string Text { get; set; }

    public UploadPdfResponse(Guid id, string text)
    {
        Id = id;
        Text = text;
    }
}
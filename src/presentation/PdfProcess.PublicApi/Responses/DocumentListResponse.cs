using PdfProcess.PublicApi.Dtos;

namespace PdfProcess.PublicApi.Responses;

public class DocumentListResponse
{
    public IReadOnlyList<DocumentDto> Items { get; set; }
    public int Total { get; set; }

    public DocumentListResponse(IReadOnlyList<DocumentDto> items, int total)
    {
        Items = items;
        Total = total;
    }
}
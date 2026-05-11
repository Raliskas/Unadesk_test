using PdfProcess.Data.Enums;

namespace PdfProcess.PublicApi.Dtos
{
    public class DocumentDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public DocumentStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public DocumentDto(Guid id, string fileName, long fileSize, DocumentStatus status, DateTime createdAt)
        {
            Id = id;
            FileName = fileName;
            FileSize = fileSize;
            Status = status;
            CreatedAt = createdAt;
        }
    }
}

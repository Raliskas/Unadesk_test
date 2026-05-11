using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PdfProcess.Data.Configs;
using PdfProcess.Data.Enums;
using PdfProcess.Data.Models;
using PdfProcess.Data.RabbitMessages;
using PdfProcess.DataAccess;
using PdfProcess.PublicApi.Dtos;
using PdfProcess.PublicApi.Responses;
using PdfProcess.RabbitMq.Interfaces;

namespace PdfProcess.PublicApi.Controllers;

public static partial class DocumentEnpoints
{
    public static IEndpointRouteBuilder MapPdfEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/documents")
            .WithTags("Documents");

        group.MapPost("/", UploadAsync)
            .WithName("UploadDocument")
            .WithSummary("Upload a pdf file")
            .DisableAntiforgery();

        group.MapGet("/", async (DocumentContext context,
                                int? page,
                                int? pageSize,
                                CancellationToken cancellationToken) =>
            {
                var actualPage = page ?? 1;
                var actualPageSize = pageSize ?? 10;

                var skip = (actualPage - 1) * actualPageSize;
                var take = actualPageSize;

                return await GetListAsync(context, skip, take, cancellationToken);
            })
            .WithName("GetDocuments")
            .WithSummary("Get list of documents with pagination");

        group.MapGet("/{id:guid}/text", GetTextAsync)
            .WithName("GetDocumentText")
            .WithSummary("Get document text");

        return app;
    }

    private static async Task<IResult> UploadAsync(IFormFile file, IOptions<StorageConfig> storageConfigOptions, IOptions<RabbitMqConfig> rabbitConfigOptions, DocumentContext context, IRabbitMqService rabbitMq, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(storageConfigOptions.Value.PdfStoragePath);

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var systemFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{timestamp}.pdf";

        var fullPath = Path.Combine(storageConfigOptions.Value.PdfStoragePath, systemFileName);

        await using (var stream = File.Create(fullPath))
            await file.CopyToAsync(stream, cancellationToken);

        var document = new Document
        {
            FileName = file.FileName,
            FilePath = fullPath,
            FileSize = file.Length,
            ContentType = file.ContentType,
            Status = DocumentStatus.Processing,
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow,
        };

        context.Documents.Add(document);
        await context.SaveChangesAsync(cancellationToken);

        var message = new DocumentMessage
        {
            DocumentId = document.Id,
            FilePath = document.FilePath,
            FileName = document.FileName,
        };

        var rabbitConfig = rabbitConfigOptions.Value;

        await rabbitMq.PublishAsync(message, rabbitConfig.QueueName, cancellationToken);

        return Results.Ok(new UploadPdfResponse(document.Id, "File accepted"));
    }

    private static async Task<IResult> GetListAsync(DocumentContext context, int skip, int take, CancellationToken cancellationToken)
    {
        var query = context.Documents.AsNoTracking();

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(x => new DocumentDto(
                x.Id, x.FileName, x.FileSize,
                x.Status, x.CreatedAt))
            .ToListAsync(cancellationToken);

        return Results.Ok(new DocumentListResponse(items, total));
    }

    private static async Task<IResult> GetTextAsync(
        Guid id,
        DocumentContext context,
        CancellationToken cancellationToken)
    {
        var document = await context.Documents
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new DocumentTextResponse(x.Id, x.FileName, x.ExtractedText))
            .FirstOrDefaultAsync(cancellationToken);

        return document is null ? Results.NotFound() : Results.Ok(document);
    }
}
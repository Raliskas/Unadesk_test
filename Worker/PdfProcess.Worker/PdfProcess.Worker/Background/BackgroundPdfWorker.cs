using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PdfProcess.Data.Enums;
using PdfProcess.Data.RabbitMessages;
using PdfProcess.Worker.DataAccess;
using PdfProcess.Worker.Services.Interfaces;

namespace PdfProcess.Worker.Background;

public class BackgroundPdfWorker : BackgroundService
{
    private readonly ILogger<BackgroundPdfWorker> _logger;
    private readonly IRabbitMqService _rabbitMqService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _queueName;
    private bool _isConsuming = false;

    public BackgroundPdfWorker(
        ILogger<BackgroundPdfWorker> logger,
        IRabbitMqService rabbitMqService,
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _rabbitMqService = rabbitMqService;
        _scopeFactory = scopeFactory;
        _queueName = configuration["RabbitMq:QueueName"] ?? "pdf_process_queue"; //TODO мог вынести в конфиги , но уже лень стало 
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Document Processing Worker started");

        try
        {
            await _rabbitMqService.StartConsumingAsync<DocumentMessage>(_queueName, async (message) =>
            {
                using var scope = _scopeFactory.CreateScope();
                
                //TODO можно было реализовать через handler чтобы не прокидывать все зависимости вот так хардкодом, а хендлер бы через DI все подтягивал 
                var context = scope.ServiceProvider.GetRequiredService<DocumentContext>();
                var pdfExtractor = scope.ServiceProvider.GetRequiredService<IPdfTextExtractor>();

                await ProcessDocumentAsync(message, context, pdfExtractor);
            });

            _logger.LogInformation("Started consuming messages from queue: {QueueName}", _queueName);

            // Waiting when cancelation token is stoped
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start message consumer");
            throw;
        }
    }

    private async Task ProcessDocumentAsync(
        DocumentMessage message,
        DocumentContext context,
        IPdfTextExtractor extractor)
    {
        _logger.LogInformation($"Processing document {message.DocumentId}: {message.FileName}");

        try
        {
            // Extract text from PDF
            var extractedText = await extractor.ExtractTextAsync(message.FilePath);

            var document = await context.Documents.FindAsync(message.DocumentId);
            if (document != null)
            {
                document.ExtractedText = extractedText;
                document.Status = DocumentStatus.Completed;
                document.ProcessedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();

                _logger.LogInformation($"Successfully processed document {message.DocumentId}. " +
                    $"Extracted {extractedText.Length} characters.");
            }
            else
            {
                _logger.LogWarning($"Document {message.DocumentId} not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to process document {message.DocumentId}");

            // update status in Failed
            var document = await context.Documents.FindAsync(message.DocumentId);
            if (document != null)
            {
                document.Status = DocumentStatus.Failed;
                document.ErrorMessage = ex.Message;
                await context.SaveChangesAsync();
            }

            throw;
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Document Processing Worker is stopping");

        if (_isConsuming)
        {
            _logger.LogInformation("Waiting for current messages to be processed...");
            await Task.Delay(5000, stoppingToken);
        }

        await base.StopAsync(stoppingToken);
    }
}
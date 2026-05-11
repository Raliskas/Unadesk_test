namespace PdfProcess.RabbitMq.Interfaces;

public interface IRabbitMqService : IAsyncDisposable
{
    Task PublishAsync<T>(T message, string queueName, CancellationToken cancellationToken) where T : class;
    Task StartConsumingAsync<T>(string queueName, Func<T, Task> onMessageReceived, CancellationToken cancellationToken) where T : class;
}

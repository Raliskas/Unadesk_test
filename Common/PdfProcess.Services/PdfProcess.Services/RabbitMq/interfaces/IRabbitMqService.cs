namespace PdfProcess.Worker.Services.Interfaces;

public interface IRabbitMqService : IDisposable
{
    Task PublishAsync<T>(T message, string queueName) where T : class;
    Task StartConsumingAsync<T>(string queueName, Func<T, Task> onMessageReceived) where T : class;
}

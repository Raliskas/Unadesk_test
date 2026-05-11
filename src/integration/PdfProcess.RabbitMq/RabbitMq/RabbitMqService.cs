using Microsoft.Extensions.Logging;
using PdfProcess.RabbitMq.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace PdfProcess.RabbitMq;

public class RabbitMqService : IRabbitMqService
{
    private readonly IConnection _connection;
    private IChannel? _channel;
    private readonly ILogger<RabbitMqService> _logger;

    public RabbitMqService(IConnection connection, ILogger<RabbitMqService> logger)
    {
        _connection = connection;
        _logger = logger;

        _logger.LogInformation("RabbitMqService initialized");
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken = default)
    {
        if (_channel == null || !_channel.IsOpen)
        {
            if (_channel?.IsOpen == false)
            {
                _logger.LogWarning("Channel was closed, creating new channel");
            }

            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            _logger.LogDebug("New RabbitMQ channel created");
        }
        return _channel;
    }

    public async Task PublishAsync<T>(T message, string queueName, CancellationToken cancellationToken) where T : class
    {
        try
        {
            _logger.LogInformation("Publishing message to queue {QueueName}, Type: {MessageType}", queueName, typeof(T).Name);

            var channel = await GetChannelAsync();
            await channel.QueueDeclareAsync(queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
            var basicProperties = new BasicProperties();

            await channel.BasicPublishAsync(exchange: "",
                routingKey: queueName,
                mandatory: false,
                basicProperties: basicProperties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogDebug("Message published successfully to {QueueName}, Size: {Size} bytes", queueName, body.Length);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to queue {QueueName}", queueName);
            throw;
        }
    }

    public async Task StartConsumingAsync<T>(string queueName, Func<T, Task> onMessageReceived, CancellationToken cancellationToken) where T : class
    {
        try
        {
            _logger.LogInformation("Starting consumer for queue {QueueName}, Type: {MessageType}", queueName, typeof(T).Name);

            var channel = await GetChannelAsync(cancellationToken);

            await channel.QueueDeclareAsync(queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(channel);

            var messageCount = await channel.MessageCountAsync(queueName, cancellationToken);
            _logger.LogInformation("Queue {QueueName} has {MessageCount} pending messages", queueName, messageCount);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var deliveryTag = ea.DeliveryTag;
                try
                {
                    // Check cancel
                    if (cancellationToken.IsCancellationRequested)
                    {
                        await channel.BasicNackAsync(deliveryTag, false, true, cancellationToken);
                        _logger.LogWarning("Cancellation requested, message requeued");
                        return;
                    }

                    var body = ea.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<T>(messageJson);

                    _logger.LogDebug("Message received from {QueueName}, DeliveryTag: {DeliveryTag}, Size: {Size} bytes",
                        queueName, deliveryTag, body.Length);

                    if (message != null)
                    {
                        await onMessageReceived(message);
                        await channel.BasicAckAsync(deliveryTag, false, cancellationToken);
                        _logger.LogDebug("Message {DeliveryTag} acknowledged successfully", deliveryTag);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize message from {QueueName}, DeliveryTag: {DeliveryTag}",
                            queueName, deliveryTag);
                        await channel.BasicNackAsync(deliveryTag, false, false, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from {QueueName}, DeliveryTag: {DeliveryTag}", queueName, deliveryTag);
                    await channel.BasicNackAsync(deliveryTag, false, true, cancellationToken);
                }
            };

            var consumerTag = await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);

            _logger.LogInformation("Consumer started successfully for queue {QueueName}, ConsumerTag: {ConsumerTag}", queueName, consumerTag);

            // Waiting cancel signal
            var tcs = new TaskCompletionSource<bool>();
            using var registration = cancellationToken.Register(() => tcs.TrySetResult(true));
            await tcs.Task;

            // Canceled consumer
            await channel.BasicCancelAsync(consumerTag, cancellationToken: cancellationToken);
            _logger.LogInformation("Consumer stopped for queue {QueueName}", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start consumer for queue {QueueName}", queueName);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("Disposing RabbitMqService");

        if (_channel != null)
        {
            _logger.LogDebug("Closing and disposing channel");
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
        }

        if (_connection != null && _connection.IsOpen)
        {
            _logger.LogDebug("Closing connection");
            await _connection.CloseAsync();
        }

        _logger.LogInformation("RabbitMqService disposed successfully");
    }
}
using Microsoft.Extensions.Logging;
using PdfProcess.Worker.Services.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace PdfProcess.Worker.Services;

public class RabbitMqService : IRabbitMqService
{
    private readonly IConnection _connection;
    private IModel? _channel;
    private readonly ILogger<RabbitMqService> _logger;

    public RabbitMqService(IConnection connection, ILogger<RabbitMqService> logger)
    {
        _connection = connection;
        _logger = logger;

        _logger.LogInformation("RabbitMqService initialized");
    }

    private IModel GetChannel()
    {
        if (_channel == null || _channel.IsClosed)
        {
            if (_channel?.IsClosed == true)
            {
                _logger.LogWarning("Channel was closed, creating new channel");
            }

            _channel = _connection.CreateModel();
            _logger.LogDebug("New RabbitMQ channel created");
        }
        return _channel;
    }

    public async Task PublishAsync<T>(T message, string queueName) where T : class
    {
        try
        {
            _logger.LogInformation("Publishing message to queue {QueueName}, Type: {MessageType}", queueName, typeof(T).Name);

            var channel = GetChannel();
            channel.QueueDeclare(queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(exchange: "",
                routingKey: queueName,
                basicProperties: null,
                body: body);

            _logger.LogDebug("Message published successfully to {QueueName}, Size: {Size} bytes", queueName, body.Length);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to queue {QueueName}", queueName);
            throw;
        }
    }

    public async Task StartConsumingAsync<T>(string queueName, Func<T, Task> onMessageReceived) where T : class
    {
        try
        {
            _logger.LogInformation("Starting consumer for queue {QueueName}, Type: {MessageType}", queueName, typeof(T).Name);

            var channel = GetChannel();
            channel.QueueDeclare(queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);

            var messageCount = channel.MessageCount(queueName);
            _logger.LogInformation("Queue {QueueName} has {MessageCount} pending messages", queueName, messageCount);

            consumer.Received += async (model, ea) =>
            {
                var deliveryTag = ea.DeliveryTag;
                try
                {
                    var body = ea.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<T>(messageJson);

                    _logger.LogDebug("Message received from {QueueName}, DeliveryTag: {DeliveryTag}, Size: {Size} bytes",
                        queueName, deliveryTag, body.Length);

                    if (message != null)
                    {
                        await onMessageReceived(message);
                        channel.BasicAck(deliveryTag, false);
                        _logger.LogDebug("Message {DeliveryTag} acknowledged successfully", deliveryTag);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize message from {QueueName}, DeliveryTag: {DeliveryTag}, Raw: {RawMessage}",
                            queueName, deliveryTag, messageJson);
                        channel.BasicNack(deliveryTag, false, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from {QueueName}, DeliveryTag: {DeliveryTag}", queueName, deliveryTag);

                    channel.BasicNack(deliveryTag, false, true);
                }
            };

            channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

            _logger.LogInformation("Consumer started successfully for queue {QueueName}", queueName);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start consumer for queue {QueueName}", queueName);
            throw;
        }
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing RabbitMqService");

        if (_channel != null)
        {
            _logger.LogDebug("Closing and disposing channel");
            _channel.Close();
            _channel.Dispose();
        }

        if (_connection != null && _connection.IsOpen)
        {
            _logger.LogDebug("Closing connection");
            _connection.Close();
            _connection.Dispose();
        }

        _logger.LogInformation("RabbitMqService disposed successfully");
    }
}
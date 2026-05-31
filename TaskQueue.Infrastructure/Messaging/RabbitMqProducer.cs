using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using TaskQueue.Core.Interfaces;
using TaskQueue.Core.Messaging;

namespace TaskQueue.Infrastructure.Messaging;

public class RabbitMqProducer : IQueueProducer, IDisposable
{
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqProducer> _logger;

    public RabbitMqProducer(
        IRabbitMqConnectionFactory connectionFactory,
        ILogger<RabbitMqProducer> logger)
    {
        _logger = logger;
        _channel = connectionFactory.CreateConnection().CreateModel();

        // rmq potwierdza przyjęcie wiadomości.
        _channel.ConfirmSelect();
    }

    public Task PublishAsync(JobMessage message, CancellationToken ct = default)
    {
        var routingKey = QueueConstants.GetRoutingKey(message.Priority);
        return PublishInternalAsync(message, routingKey);
    }

    public Task PublishRetryAsync(JobMessage message, TimeSpan delay, CancellationToken ct = default)
    {
        var routingKey = QueueConstants.GetRetryRoutingKey(message.Priority);
        return PublishInternalAsync(message, routingKey, delay);
    }

    private Task PublishInternalAsync(
        JobMessage message,
        string routingKey,
        TimeSpan? delay = null)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        var props = _channel.CreateBasicProperties();
        props.Persistent = true;
        props.ContentType = "application/json";
        props.MessageId = message.JobId.ToString();
        props.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        props.Headers = new Dictionary<string, object>
        {
            ["retry-count"] = message.RetryCount,
            ["job-type"] = message.Type
        };

        if (delay is { } retryDelay)
        {
            var ttl = Math.Max(1, (int)retryDelay.TotalMilliseconds);
            props.Expiration = ttl.ToString(CultureInfo.InvariantCulture);
        }

        _channel.BasicPublish(
            exchange: QueueConstants.ExchangeName,
            routingKey: routingKey,
            basicProperties: props,
            body: body);

        _channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));

        _logger.LogInformation(
            "Published job {JobId} ({Type}) with routing key '{RoutingKey}'",
            message.JobId, message.Type, routingKey);

        return Task.CompletedTask;
    }

    public void Dispose() => _channel.Dispose();
}

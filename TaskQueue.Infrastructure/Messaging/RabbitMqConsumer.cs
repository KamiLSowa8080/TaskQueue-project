using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TaskQueue.Core.Interfaces;
using TaskQueue.Core.Messaging;

namespace TaskQueue.Infrastructure.Messaging;

public class RabbitMqConsumer : IQueueConsumer, IDisposable
{
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqConsumer> _logger;

    public RabbitMqConsumer(
        IRabbitMqConnectionFactory connectionFactory,
        ILogger<RabbitMqConsumer> logger)
    {
        _logger = logger;
        _channel = connectionFactory.CreateConnection().CreateModel();

        // pobiera 1 wiadomość na raz; worker nie dostanie kolejnej dopóki nie ackuje
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
    }

    public void StartConsuming(
        string queueName,
        Func<JobMessage, CancellationToken, Task<bool>> handler,
        CancellationToken ct = default)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (_, args) =>
        {
            JobMessage? message = null;
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.Span);
                message = JsonSerializer.Deserialize<JobMessage>(json);

                if (message is null)
                {
                    _logger.LogWarning("Received null message from queue {Queue}", queueName);
                    _channel.BasicNack(args.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                _logger.LogInformation(
                    "Received job {JobId} ({Type}) from queue '{Queue}'",
                    message.JobId, message.Type, queueName);

                var success = await handler(message, ct);

                if (success)
                    _channel.BasicAck(args.DeliveryTag, multiple: false);
                else
                    _channel.BasicNack(args.DeliveryTag, multiple: false, requeue: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing message {JobId} from queue '{Queue}'",
                    message?.JobId, queueName);

                _channel.BasicNack(args.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(
            queue: queueName,
            autoAck: false, // ręcznie ackujemy po przetworzeniu
            consumer: consumer);

        _logger.LogInformation("Started consuming queue '{Queue}'", queueName);
    }

    public void Dispose() => _channel.Dispose();
}
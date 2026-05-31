using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using TaskQueue.Core.Messaging;

namespace TaskQueue.Infrastructure.Messaging;

public class RabbitMqTopologyInitializer
{
    private const string DeadLetterExchangeName = "task_queue_dead_letter_exchange";

    private readonly IRabbitMqConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMqTopologyInitializer> _logger;

    public RabbitMqTopologyInitializer(
        IRabbitMqConnectionFactory connectionFactory,
        ILogger<RabbitMqTopologyInitializer> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public void Initialize()
    {
        using var connection = _connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(
            exchange: DeadLetterExchangeName,
            type: ExchangeType.Direct,
            durable: true);

        channel.QueueDeclare(
            queue: QueueConstants.DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        channel.QueueBind(
            queue: QueueConstants.DeadLetterQueue,
            exchange: DeadLetterExchangeName,
            routingKey: QueueConstants.DeadLetterQueue);

        channel.ExchangeDeclare(
            exchange: QueueConstants.ExchangeName,
            type: ExchangeType.Direct,
            durable: true);

        var queueArgs = new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = DeadLetterExchangeName,
            ["x-dead-letter-routing-key"] = QueueConstants.DeadLetterQueue
        };

        DeclareAndBindQueue(channel, QueueConstants.HighPriorityQueue, QueueConstants.HighPriorityKey, queueArgs);
        DeclareAndBindQueue(channel, QueueConstants.NormalPriorityQueue, QueueConstants.NormalPriorityKey, queueArgs);
        DeclareAndBindQueue(channel, QueueConstants.LowPriorityQueue, QueueConstants.LowPriorityKey, queueArgs);

        DeclareRetryQueue(
            channel,
            QueueConstants.HighPriorityRetryQueue,
            QueueConstants.HighPriorityRetryKey,
            QueueConstants.HighPriorityKey);

        DeclareRetryQueue(
            channel,
            QueueConstants.NormalPriorityRetryQueue,
            QueueConstants.NormalPriorityRetryKey,
            QueueConstants.NormalPriorityKey);

        DeclareRetryQueue(
            channel,
            QueueConstants.LowPriorityRetryQueue,
            QueueConstants.LowPriorityRetryKey,
            QueueConstants.LowPriorityKey);

        _logger.LogInformation("RabbitMQ topology initialized successfully");
    }

    private static void DeclareAndBindQueue(
        IModel channel,
        string queueName,
        string routingKey,
        IDictionary<string, object> args)
    {
        channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: args);

        channel.QueueBind(
            queue: queueName,
            exchange: QueueConstants.ExchangeName,
            routingKey: routingKey);
    }

    private static void DeclareRetryQueue(
        IModel channel,
        string queueName,
        string retryRoutingKey,
        string targetRoutingKey)
    {
        var args = new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = QueueConstants.ExchangeName,
            ["x-dead-letter-routing-key"] = targetRoutingKey
        };

        channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: args);

        channel.QueueBind(
            queue: queueName,
            exchange: QueueConstants.ExchangeName,
            routingKey: retryRoutingKey);
    }
}

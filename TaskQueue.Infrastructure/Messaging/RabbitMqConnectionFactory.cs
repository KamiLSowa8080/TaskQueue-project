using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TaskQueue.Infrastructure.Settings;

namespace TaskQueue.Infrastructure.Messaging;

public interface IRabbitMqConnectionFactory
{
    IConnection CreateConnection();
}

public class RabbitMqConnectionFactory : IRabbitMqConnectionFactory, IDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqConnectionFactory> _logger;
    private IConnection? _connection;
    private readonly object _lock = new();

    public RabbitMqConnectionFactory(
        IOptions<RabbitMqSettings> settings,
        ILogger<RabbitMqConnectionFactory> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    // single conect jeden na proces
    public IConnection CreateConnection()
    {
        lock (_lock)
        {
            if (_connection is { IsOpen: true })
                return _connection;

            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,        // auto-reconnect
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection("TaskQueueSystem");
            _logger.LogInformation("RabbitMQ connection established to {Host}", _settings.Host);

            return _connection;
        }
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}

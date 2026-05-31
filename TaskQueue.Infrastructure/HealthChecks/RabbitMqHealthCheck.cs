using Microsoft.Extensions.Diagnostics.HealthChecks;
using TaskQueue.Infrastructure.Messaging;

namespace TaskQueue.Infrastructure.HealthChecks;

public class RabbitMqHealthCheck : IHealthCheck
{
    private readonly IRabbitMqConnectionFactory _connectionFactory;

    public RabbitMqHealthCheck(IRabbitMqConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connectionFactory.CreateConnection();

            return Task.FromResult(connection.IsOpen
                ? HealthCheckResult.Healthy("RabbitMQ connection is open")
                : HealthCheckResult.Unhealthy("RabbitMQ connection is closed"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy("RabbitMQ unreachable", ex));
        }
    }
}
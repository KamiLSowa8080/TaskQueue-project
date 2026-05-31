using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskQueue.Core.Interfaces;
using TaskQueue.Infrastructure.Data;
using TaskQueue.Infrastructure.HealthChecks;
using TaskQueue.Infrastructure.Messaging;
using TaskQueue.Infrastructure.Repositories;
using TaskQueue.Infrastructure.Services;
using TaskQueue.Infrastructure.Settings;

namespace TaskQueue.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("PostgreSQL"),
                npgsql => npgsql
                    .MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
                    .EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null)
            )
        );

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJobRepository, JobRepository>();

        services.Configure<RabbitMqSettings>(options =>
            configuration.GetSection(RabbitMqSettings.SectionName).Bind(options));

        services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
        services.AddSingleton<RabbitMqTopologyInitializer>();
        services.AddScoped<IQueueProducer, RabbitMqProducer>();
        services.AddScoped<IQueueConsumer, RabbitMqConsumer>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IQueueStatsService, QueueStatsService>();
        services.TryAddSingleton<IJobNotifier, NoOpJobNotifier>();

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(
                name: "postgresql",
                tags: ["db", "infrastructure"])
            .AddCheck<RabbitMqHealthCheck>(
                name: "rabbitmq",
                tags: ["messaging", "infrastructure"]);

        return services;
    }
}

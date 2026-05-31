using Microsoft.EntityFrameworkCore;
using TaskQueue.Core.Interfaces;
using TaskQueue.Infrastructure;
using TaskQueue.Infrastructure.Data;
using TaskQueue.Infrastructure.Messaging;
using TaskQueue.Worker.Handlers;
using TaskQueue.Worker.Workers;
using TaskQueue.Worker.Settings;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection(SmtpSettings.SectionName));

builder.Services.AddSingleton<IJobHandler, SendEmailHandler>();
builder.Services.AddSingleton<IJobHandler, GenerateReportHandler>();

builder.Services.AddSingleton<JobProcessor>();
builder.Services.AddHostedService<JobWorkerService>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var topology = scope.ServiceProvider.GetRequiredService<RabbitMqTopologyInitializer>();
    topology.Initialize();
}

await host.RunAsync();

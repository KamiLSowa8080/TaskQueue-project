using System.Net;
using System.Net.Mail;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskQueue.Core.Interfaces;
using TaskQueue.Core.Messaging;
using TaskQueue.Worker.Settings;

namespace TaskQueue.Worker.Handlers;

public record SendEmailPayload(
    string To,
    string Subject,
    string? Body = null
);

public class SendEmailHandler : IJobHandler
{
    public string JobType => "SendEmail";

    private readonly SmtpSettings _settings;
    private readonly ILogger<SendEmailHandler> _logger;

    public SendEmailHandler(
        IOptions<SmtpSettings> settings,
        ILogger<SendEmailHandler> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(JobMessage message, CancellationToken ct = default)
    {
        ValidateSettings();

        var payload = JsonSerializer.Deserialize<SendEmailPayload>(
            message.Payload,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (payload is null || string.IsNullOrWhiteSpace(payload.To))
            throw new InvalidOperationException("Invalid email payload. Field 'to' is required.");

        if (string.IsNullOrWhiteSpace(payload.Subject))
            throw new InvalidOperationException("Invalid email payload. Field 'subject' is required.");

        _logger.LogInformation(
            "Sending email for job {JobId} to {To} using SMTP {Host}:{Port} SSL={UseSsl}",
            message.JobId,
            payload.To,
            _settings.Host,
            _settings.Port,
            _settings.UseSsl);

        using var smtp = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.UseSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_settings.User, _settings.Pass),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 30000
        };

        using var mail = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = payload.Subject,
            Body = payload.Body ?? string.Empty,
            IsBodyHtml = false
        };

        mail.To.Add(payload.To);

        await smtp.SendMailAsync(mail, ct);

        _logger.LogInformation(
            "Email sent for job {JobId} to {To}",
            message.JobId,
            payload.To);

        return true;
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.Host))
            throw new InvalidOperationException("SMTP host is missing.");

        if (_settings.Port <= 0)
            throw new InvalidOperationException("SMTP port is invalid.");

        if (string.IsNullOrWhiteSpace(_settings.User))
            throw new InvalidOperationException("SMTP user is missing.");

        if (string.IsNullOrWhiteSpace(_settings.Pass))
            throw new InvalidOperationException("SMTP password is missing.");

        if (string.IsNullOrWhiteSpace(_settings.FromEmail))
            throw new InvalidOperationException("SMTP from email is missing.");
    }
}

namespace TaskQueue.Worker.Settings;

public class SmtpSettings
{
    public const string SectionName = "Smtp";

    public string Host { get; init; } = "smtp.gmail.com";
    public int Port { get; init; } = 587;
    public bool UseSsl { get; init; } = true;
    public string User { get; init; } = string.Empty;
    public string Pass { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
}

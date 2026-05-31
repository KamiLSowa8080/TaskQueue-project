namespace TaskQueue.Core.Services;

public static class RetryPolicy
{
    /// <summary>
    /// exponential backoff z jitterem ktory zapobiega thundering herd
    /// retry 1 = ok.2s, retry 2 = ok.4s, retry 3 = ok8s
    /// </summary>
    public static TimeSpan GetDelay(int retryCount)
    {
        var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
        var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
        var maxDelay = TimeSpan.FromMinutes(10);

        return baseDelay + jitter > maxDelay ? maxDelay : baseDelay + jitter;
    }
}
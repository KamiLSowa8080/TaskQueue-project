namespace TaskQueue.Core.Messaging;

public static class QueueConstants
{
    public const string ExchangeName = "task_queue_exchange";

    // Kolejki według priorytetu
    public const string HighPriorityQueue = "jobs.high";
    public const string NormalPriorityQueue = "jobs.normal";
    public const string LowPriorityQueue = "jobs.low";
    public const string HighPriorityRetryQueue = "jobs.high.retry";
    public const string NormalPriorityRetryQueue = "jobs.normal.retry";
    public const string LowPriorityRetryQueue = "jobs.low.retry";
    public const string DeadLetterQueue = "jobs.dead_letter"; // tu trafia niezdatne do naprawy

    // routing keys
    public const string HighPriorityKey = "job.priority.high";
    public const string NormalPriorityKey = "job.priority.normal";
    public const string LowPriorityKey = "job.priority.low";
    public const string HighPriorityRetryKey = "job.retry.high";
    public const string NormalPriorityRetryKey = "job.retry.normal";
    public const string LowPriorityRetryKey = "job.retry.low";

    public static string GetRoutingKey(int priority) => priority switch
    {
        >= 10 => HighPriorityKey,
        >= 5 => NormalPriorityKey,
        _ => LowPriorityKey
    };

    public static string GetQueueName(int priority) => priority switch
    {
        >= 10 => HighPriorityQueue,
        >= 5 => NormalPriorityQueue,
        _ => LowPriorityQueue
    };

    public static string GetRetryRoutingKey(int priority) => priority switch
    {
        >= 10 => HighPriorityRetryKey,
        >= 5 => NormalPriorityRetryKey,
        _ => LowPriorityRetryKey
    };
}

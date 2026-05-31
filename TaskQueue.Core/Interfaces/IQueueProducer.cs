using TaskQueue.Core.Messaging;

namespace TaskQueue.Core.Interfaces;

public interface IQueueProducer
{
    Task PublishAsync(JobMessage message, CancellationToken ct = default);
    Task PublishRetryAsync(JobMessage message, TimeSpan delay, CancellationToken ct = default);
}
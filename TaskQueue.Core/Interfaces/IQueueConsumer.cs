using TaskQueue.Core.Messaging;

namespace TaskQueue.Core.Interfaces;

public interface IQueueConsumer
{
    void StartConsuming(
        string queueName,
        Func<JobMessage, CancellationToken, Task<bool>> handler, // true = ack false = nack
        CancellationToken ct = default);
}
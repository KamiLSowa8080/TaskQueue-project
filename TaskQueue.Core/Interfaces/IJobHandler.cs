using TaskQueue.Core.Messaging;

namespace TaskQueue.Core.Interfaces;

public interface IJobHandler
{
    string JobType { get; }  // np sendemail
    Task<bool> HandleAsync(JobMessage message, CancellationToken ct = default);
}
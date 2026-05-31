namespace TaskQueue.Core.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IJobRepository Jobs { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
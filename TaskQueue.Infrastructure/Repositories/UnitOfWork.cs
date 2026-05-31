using TaskQueue.Core.Interfaces;
using TaskQueue.Infrastructure.Data;

namespace TaskQueue.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IJobRepository? _jobs;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    // tworzy repo dopiero gdy potrzebne
    public IJobRepository Jobs => _jobs ??= new JobRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public async ValueTask DisposeAsync()
        => await _context.DisposeAsync();
}
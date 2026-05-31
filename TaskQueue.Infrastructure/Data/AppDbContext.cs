using Microsoft.EntityFrameworkCore;
using TaskQueue.Core.Models;

namespace TaskQueue.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<JobTask> JobTasks => Set<JobTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // automatycznie załaduje wszystkie IEntityTypeConfiguration z tego assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }
}
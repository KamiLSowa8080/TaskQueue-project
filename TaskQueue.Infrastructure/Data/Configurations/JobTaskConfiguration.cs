using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskQueue.Core.Models;

namespace TaskQueue.Infrastructure.Data.Configurations;

public class JobTaskConfiguration : IEntityTypeConfiguration<JobTask>
{
    public void Configure(EntityTypeBuilder<JobTask> builder)
    {
        builder.ToTable("job_tasks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // my generujemy Guid, nie baza

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb") // PostgreSQL jsonb — szybsze od json
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>() // w bazie trzymamy "Pending", nie 0
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .HasDefaultValue(0);

        builder.Property(x => x.RetryCount)
            .HasColumnName("retry_count")
            .HasDefaultValue(0);

        builder.Property(x => x.MaxRetries)
            .HasColumnName("max_retries")
            .HasDefaultValue(3);

        builder.Property(x => x.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.StartedAt)
            .HasColumnName("started_at");

        builder.Property(x => x.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(x => x.NextRetryAt)
            .HasColumnName("next_retry_at");

        // Indeksy — kluczowe dla wydajności w produkcji
        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_job_tasks_status");

        builder.HasIndex(x => x.Priority)
            .HasDatabaseName("ix_job_tasks_priority");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_job_tasks_created_at");

        builder.HasIndex(x => new { x.Status, x.Priority })
            .HasDatabaseName("ix_job_tasks_status_priority"); // composite — worker to odpyta
    }
}
using Microsoft.EntityFrameworkCore;

namespace Laundry.Infrastructure.Persistence;

/// <summary>
/// PostgreSQL schema root. This context is used by both local-admin and cloud API
/// deployments; connection strings select the target database.
/// </summary>
public sealed class LaundryDbContext(DbContextOptions<LaundryDbContext> options) : DbContext(options)
{
    public DbSet<CustomerRecord> Customers => Set<CustomerRecord>();
    public DbSet<OrderRecord> Orders => Set<OrderRecord>();
    public DbSet<SyncOutboxRecord> SyncOutbox => Set<SyncOutboxRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerRecord>(entity =>
        {
            entity.ToTable("customers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.WhatsAppNumber).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.WhatsAppNumber).IsUnique();
            entity.Property(x => x.DisplayName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
        });
        modelBuilder.Entity<OrderRecord>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OrderNumber).HasMaxLength(48).IsRequired();
            entity.HasIndex(x => x.OrderNumber).IsUnique();
            entity.Property(x => x.FinalTotal).HasPrecision(18, 2);
            entity.Property(x => x.SyncVersion).IsConcurrencyToken();
            entity.HasOne<CustomerRecord>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<SyncOutboxRecord>(entity =>
        {
            entity.ToTable("sync_outbox");
            entity.HasKey(x => x.EventId);
            entity.Property(x => x.EventType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => new { x.Status, x.CreatedAtUtc });
        });
    }
}

public sealed class CustomerRecord
{
    public Guid Id { get; set; }
    public required string DisplayName { get; set; }
    public required string WhatsAppNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; }
}

public sealed class OrderRecord
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public required string OrderNumber { get; set; }
    public decimal FinalTotal { get; set; }
    public required string ProcessStatus { get; set; }
    public required string PaymentStatus { get; set; }
    public long SyncVersion { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}

public sealed class SyncOutboxRecord
{
    public Guid EventId { get; set; }
    public required string EventType { get; set; }
    public required string IdempotencyKey { get; set; }
    public required string PayloadJson { get; set; }
    public required string Status { get; set; }
    public int RetryCount { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? LastAttemptAtUtc { get; set; }
}

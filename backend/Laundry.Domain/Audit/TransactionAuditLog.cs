namespace Laundry.Domain.Audit;

public enum TransactionAuditAction { OrderChanged, PaymentRecorded, RefundRecorded, CreditChanged }

public sealed record TransactionAuditLog
{
    public TransactionAuditLog(Guid id, Guid entityId, TransactionAuditAction action, Guid actorId, string correlationId, string source, string? beforeJson, string? afterJson, DateTimeOffset occurredAtUtc)
    {
        if (entityId == Guid.Empty) throw new ArgumentException("Entity ID wajib diisi.", nameof(entityId));
        if (actorId == Guid.Empty) throw new ArgumentException("Actor ID wajib diisi.", nameof(actorId));
        if (string.IsNullOrWhiteSpace(correlationId)) throw new ArgumentException("Correlation ID wajib diisi.", nameof(correlationId));
        if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("Sumber wajib diisi.", nameof(source));
        Id = id; EntityId = entityId; Action = action; ActorId = actorId; CorrelationId = correlationId; Source = source; BeforeJson = beforeJson; AfterJson = afterJson; OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; }
    public Guid EntityId { get; }
    public TransactionAuditAction Action { get; }
    public Guid ActorId { get; }
    public string CorrelationId { get; }
    public string Source { get; }
    public string? BeforeJson { get; }
    public string? AfterJson { get; }
    public DateTimeOffset OccurredAtUtc { get; }
}

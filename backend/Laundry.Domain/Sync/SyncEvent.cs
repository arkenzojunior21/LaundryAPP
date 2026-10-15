namespace Laundry.Domain.Sync;

public enum SyncEventStatus { Pending, Processing, Sent, Failed }

public sealed class SyncEvent
{
    public SyncEvent(Guid eventId, string eventType, string idempotencyKey, string payloadJson, DateTimeOffset createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentException("Jenis event wajib diisi.", nameof(eventType));
        if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new ArgumentException("Idempotency key wajib diisi.", nameof(idempotencyKey));
        EventId = eventId;
        EventType = eventType;
        IdempotencyKey = idempotencyKey;
        PayloadJson = payloadJson;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid EventId { get; }
    public string EventType { get; }
    public string IdempotencyKey { get; }
    public string PayloadJson { get; }
    public DateTimeOffset CreatedAtUtc { get; }
    public int RetryCount { get; private set; }
    public SyncEventStatus Status { get; private set; } = SyncEventStatus.Pending;
    public DateTimeOffset? NextAttemptAtUtc { get; private set; }
    public string? LastError { get; private set; }

    public void MarkSent() { Status = SyncEventStatus.Sent; LastError = null; NextAttemptAtUtc = null; }
    public void MarkFailed(string error, DateTimeOffset now)
    {
        RetryCount++;
        LastError = error;
        if (RetryCount >= 5) { Status = SyncEventStatus.Failed; NextAttemptAtUtc = null; return; }
        Status = SyncEventStatus.Pending;
        NextAttemptAtUtc = now.AddSeconds(Math.Pow(2, RetryCount));
    }
    public void RetryManually() { Status = SyncEventStatus.Pending; RetryCount = 0; NextAttemptAtUtc = null; LastError = null; }
}

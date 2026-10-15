using System.Collections.Concurrent;
using Laundry.Application.Sync;
using Laundry.Domain.Sync;

namespace Laundry.Infrastructure.Sync;

/// <summary>Development substitute for the PostgreSQL outbox table.</summary>
public sealed class InMemorySyncOutbox : ISyncOutbox
{
    private readonly ConcurrentDictionary<Guid, SyncEvent> _events = new();
    public void Enqueue(SyncEvent syncEvent) => _events.TryAdd(syncEvent.EventId, syncEvent);
    public Task<IReadOnlyList<SyncEvent>> GetReadyAsync(DateTimeOffset now, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<SyncEvent>>(_events.Values.Where(x => x.Status == SyncEventStatus.Pending && (!x.NextAttemptAtUtc.HasValue || x.NextAttemptAtUtc <= now)).ToArray());
}

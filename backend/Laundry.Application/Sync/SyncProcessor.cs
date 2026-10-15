using Laundry.Domain.Sync;

namespace Laundry.Application.Sync;

public interface ISyncOutbox
{
    Task<IReadOnlyList<SyncEvent>> GetReadyAsync(DateTimeOffset now, CancellationToken cancellationToken);
}

public interface ISyncTransport
{
    Task DeliverAsync(SyncEvent syncEvent, CancellationToken cancellationToken);
}

public sealed class SyncProcessor(ISyncOutbox outbox, ISyncTransport transport, TimeProvider clock)
{
    public async Task<SyncRunResult> ProcessAsync(CancellationToken cancellationToken)
    {
        var events = await outbox.GetReadyAsync(clock.GetUtcNow(), cancellationToken);
        var sent = 0;
        var failed = 0;
        foreach (var syncEvent in events.OrderBy(x => x.CreatedAtUtc))
        {
            try { await transport.DeliverAsync(syncEvent, cancellationToken); syncEvent.MarkSent(); sent++; }
            catch (Exception exception) when (exception is not OperationCanceledException) { syncEvent.MarkFailed(exception.Message, clock.GetUtcNow()); failed++; }
        }
        return new SyncRunResult(sent, failed, events.Count);
    }
}

public sealed record SyncRunResult(int Sent, int Failed, int Attempted);

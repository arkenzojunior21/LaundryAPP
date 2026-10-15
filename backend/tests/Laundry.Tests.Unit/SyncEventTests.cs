using Laundry.Domain.Sync;
using Xunit;

namespace Laundry.Tests.Unit;

public sealed class SyncEventTests
{
    [Fact]
    public void FiveFailures_LeavesEventVisibleAsFailed()
    {
        var now = new DateTimeOffset(2026, 10, 15, 12, 0, 0, TimeSpan.Zero);
        var syncEvent = new SyncEvent(Guid.NewGuid(), "order.changed", "event-1", "{}", now);
        for (var attempt = 0; attempt < 5; attempt++) syncEvent.MarkFailed("offline", now);
        Assert.Equal(SyncEventStatus.Failed, syncEvent.Status);
        Assert.Equal(5, syncEvent.RetryCount);
        Assert.Equal("offline", syncEvent.LastError);
    }

    [Fact]
    public void ManualRetry_ReturnsFailedEventToPending()
    {
        var now = DateTimeOffset.UtcNow;
        var syncEvent = new SyncEvent(Guid.NewGuid(), "order.changed", "event-2", "{}", now);
        for (var attempt = 0; attempt < 5; attempt++) syncEvent.MarkFailed("offline", now);
        syncEvent.RetryManually();
        Assert.Equal(SyncEventStatus.Pending, syncEvent.Status);
        Assert.Equal(0, syncEvent.RetryCount);
        Assert.Null(syncEvent.LastError);
    }
}

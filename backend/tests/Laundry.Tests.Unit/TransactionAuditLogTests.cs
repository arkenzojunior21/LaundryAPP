using Laundry.Domain.Audit;
using Xunit;

namespace Laundry.Tests.Unit;

public sealed class TransactionAuditLogTests
{
    [Fact]
    public void AuditRecord_PreservesImmutableTransactionContext()
    {
        var entityId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var audit = new TransactionAuditLog(Guid.NewGuid(), entityId, TransactionAuditAction.PaymentRecorded, actorId, "corr-123", "local", "{\"outstanding\":10000}", "{\"outstanding\":0}", DateTimeOffset.UtcNow);
        Assert.Equal(entityId, audit.EntityId);
        Assert.Equal(TransactionAuditAction.PaymentRecorded, audit.Action);
        Assert.Equal("corr-123", audit.CorrelationId);
    }
}

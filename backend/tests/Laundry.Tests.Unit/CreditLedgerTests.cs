using Laundry.Domain.Finance;
using Xunit;

namespace Laundry.Tests.Unit;

public sealed class CreditLedgerTests
{
    [Fact]
    public void DuplicateCredit_IsIgnoredAndBalanceIsAppendOnly()
    {
        var ledger = new CustomerCreditLedger();
        var entry = new CreditLedgerEntry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10_000m, CreditEntryType.Credit, "overpayment-1", DateTimeOffset.UtcNow);
        ledger.Append(entry);
        ledger.Append(entry with { Id = Guid.NewGuid() });
        Assert.Single(ledger.Entries);
        Assert.Equal(10_000m, ledger.Balance);
    }

    [Fact]
    public void Debit_CannotExceedAvailableCredit()
    {
        var customerId = Guid.NewGuid();
        var ledger = new CustomerCreditLedger();
        ledger.Append(new CreditLedgerEntry(Guid.NewGuid(), customerId, null, 5_000m, CreditEntryType.Credit, "credit-1", DateTimeOffset.UtcNow));
        Assert.Throws<InvalidOperationException>(() => ledger.Append(new CreditLedgerEntry(Guid.NewGuid(), customerId, null, 5_001m, CreditEntryType.Debit, "debit-1", DateTimeOffset.UtcNow)));
    }
}

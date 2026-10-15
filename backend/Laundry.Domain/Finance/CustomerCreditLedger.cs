namespace Laundry.Domain.Finance;

public enum CreditEntryType { Credit, Debit }

public sealed record CreditLedgerEntry
{
    public CreditLedgerEntry(Guid id, Guid customerId, Guid? orderId, decimal amount, CreditEntryType type, string idempotencyKey, DateTimeOffset createdAtUtc)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new ArgumentException("Idempotency key wajib diisi.", nameof(idempotencyKey));
        Id = id;
        CustomerId = customerId;
        OrderId = orderId;
        Amount = amount;
        Type = type;
        IdempotencyKey = idempotencyKey;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public Guid? OrderId { get; init; }
    public decimal Amount { get; init; }
    public CreditEntryType Type { get; init; }
    public string IdempotencyKey { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public decimal SignedAmount => Type == CreditEntryType.Credit ? Amount : -Amount;
}

public sealed class CustomerCreditLedger
{
    private readonly List<CreditLedgerEntry> _entries = [];
    public IReadOnlyCollection<CreditLedgerEntry> Entries => _entries.AsReadOnly();
    public decimal Balance => _entries.Sum(x => x.SignedAmount);

    public void Append(CreditLedgerEntry entry)
    {
        if (_entries.Any(x => x.IdempotencyKey == entry.IdempotencyKey)) return;
        if (entry.Type == CreditEntryType.Debit && entry.Amount > Balance) throw new InvalidOperationException("Saldo kredit tidak mencukupi.");
        _entries.Add(entry);
    }
}

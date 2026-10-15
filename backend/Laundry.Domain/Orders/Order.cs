namespace Laundry.Domain.Orders;

public enum OrderStatus
{
    AwaitingReview,
    Confirmed,
    AwaitingDropOff,
    Received,
    Weighed,
    AwaitingProcess,
    Processing,
    QualityCheck,
    ReadyForCollection,
    Delivering,
    Completed,
    Cancelled
}

public enum PaymentStatus { Unpaid, DepositPaid, PartiallyPaid, Paid, PartialRefund, FullRefund, FailedOrExpired }
public enum PricingUnit { Kilogram, Item, FixedPackage }

public sealed record PriceSnapshot(PricingUnit Unit, decimal UnitPrice, decimal MinimumWeightKg, decimal ExpressFee, decimal DeliveryFee);

public sealed class Order
{
    private readonly List<Payment> _payments = [];

    public Order(Guid id, string orderNumber, Guid customerId, PriceSnapshot price, decimal estimatedQuantity)
    {
        if (string.IsNullOrWhiteSpace(orderNumber)) throw new ArgumentException("Nomor pesanan wajib diisi.", nameof(orderNumber));
        if (estimatedQuantity < 0) throw new ArgumentOutOfRangeException(nameof(estimatedQuantity));
        Id = id;
        OrderNumber = orderNumber;
        CustomerId = customerId;
        Price = price;
        EstimatedQuantity = estimatedQuantity;
        Status = OrderStatus.AwaitingReview;
        Recalculate(estimatedQuantity);
    }

    public Guid Id { get; }
    public string OrderNumber { get; }
    public Guid CustomerId { get; }
    public PriceSnapshot Price { get; }
    public decimal EstimatedQuantity { get; }
    public decimal? ActualQuantity { get; private set; }
    public decimal FinalTotal { get; private set; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();
    public decimal PaidAmount => _payments.Where(p => p.IsSuccessful).Sum(p => p.Amount);
    public decimal Outstanding => Math.Max(0, FinalTotal - PaidAmount);
    public decimal CreditToCreate => Math.Max(0, PaidAmount - FinalTotal);
    public PaymentStatus CurrentPaymentStatus => PaidAmount == 0 ? global::Laundry.Domain.Orders.PaymentStatus.Unpaid : Outstanding == 0 ? global::Laundry.Domain.Orders.PaymentStatus.Paid : global::Laundry.Domain.Orders.PaymentStatus.PartiallyPaid;

    public void RecordActualQuantity(decimal actualQuantity)
    {
        if (actualQuantity < 0) throw new ArgumentOutOfRangeException(nameof(actualQuantity));
        ActualQuantity = actualQuantity;
        Recalculate(actualQuantity);
        if (Status < OrderStatus.Weighed) Status = OrderStatus.Weighed;
    }

    public void RecordPayment(Payment payment)
    {
        if (_payments.Any(p => p.IdempotencyKey == payment.IdempotencyKey)) return;
        _payments.Add(payment);
    }

    public void TransitionTo(OrderStatus next)
    {
        if (next == OrderStatus.Completed && Outstanding > 0) throw new InvalidOperationException("Pesanan tidak dapat diserahkan sebelum lunas.");
        if (next == OrderStatus.Cancelled)
        {
            if (Status >= OrderStatus.Processing) throw new InvalidOperationException("Pesanan yang sudah diproses tidak dapat dibatalkan.");
            Status = next;
            return;
        }
        if (!AllowedTransitions[Status].Contains(next)) throw new InvalidOperationException($"Transisi dari {Status} ke {next} tidak valid.");
        Status = next;
    }

    private static readonly IReadOnlyDictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new Dictionary<OrderStatus, OrderStatus[]>
    {
        [OrderStatus.AwaitingReview] = [OrderStatus.Confirmed],
        [OrderStatus.Confirmed] = [OrderStatus.AwaitingDropOff, OrderStatus.Received],
        [OrderStatus.AwaitingDropOff] = [OrderStatus.Received],
        [OrderStatus.Received] = [OrderStatus.Weighed],
        [OrderStatus.Weighed] = [OrderStatus.AwaitingProcess],
        [OrderStatus.AwaitingProcess] = [OrderStatus.Processing],
        [OrderStatus.Processing] = [OrderStatus.QualityCheck],
        [OrderStatus.QualityCheck] = [OrderStatus.ReadyForCollection],
        [OrderStatus.ReadyForCollection] = [OrderStatus.Delivering, OrderStatus.Completed],
        [OrderStatus.Delivering] = [OrderStatus.Completed],
        [OrderStatus.Completed] = [],
        [OrderStatus.Cancelled] = []
    };

    private void Recalculate(decimal quantity)
    {
        var billableQuantity = Price.Unit == PricingUnit.Kilogram ? Math.Max(quantity, Price.MinimumWeightKg) : quantity;
        FinalTotal = (billableQuantity * Price.UnitPrice) + Price.ExpressFee + Price.DeliveryFee;
    }
}

public sealed record Payment
{
    public Payment(Guid id, decimal amount, string idempotencyKey, bool isSuccessful)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new ArgumentException("Idempotency key wajib diisi.", nameof(idempotencyKey));
        Id = id;
        Amount = amount;
        IdempotencyKey = idempotencyKey;
        IsSuccessful = isSuccessful;
    }

    public Guid Id { get; }
    public decimal Amount { get; }
    public string IdempotencyKey { get; }
    public bool IsSuccessful { get; }
}

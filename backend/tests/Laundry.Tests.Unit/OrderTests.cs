using Laundry.Domain.Orders;
using Xunit;

namespace Laundry.Tests.Unit;

public sealed class OrderTests
{
    [Fact]
    public void ActualWeight_UsesMinimumWeightAndRetainsSuccessfulPayment()
    {
        var order = NewOrder();
        order.RecordPayment(new Payment(Guid.NewGuid(), 20_000m, "payment-1", true));
        order.RecordActualQuantity(1m);
        Assert.Equal(15_000m, order.FinalTotal);
        Assert.Equal(20_000m, order.PaidAmount);
        Assert.Equal(5_000m, order.CreditToCreate);
    }

    [Fact]
    public void Completion_IsRejectedWhenOrderIsNotPaid()
    {
        var order = NewOrder();
        Assert.Throws<InvalidOperationException>(() => order.TransitionTo(OrderStatus.Completed));
    }

    [Fact]
    public void DuplicatePayment_IsIdempotent()
    {
        var order = NewOrder();
        order.RecordPayment(new Payment(Guid.NewGuid(), 5_000m, "key-1", true));
        order.RecordPayment(new Payment(Guid.NewGuid(), 5_000m, "key-1", true));
        Assert.Single(order.Payments);
    }

    [Fact]
    public void StatusMachine_RejectsSkippedAndLateCancellationTransitions()
    {
        var order = NewOrder();
        Assert.Throws<InvalidOperationException>(() => order.TransitionTo(OrderStatus.Processing));
        order.TransitionTo(OrderStatus.Confirmed);
        order.TransitionTo(OrderStatus.AwaitingDropOff);
        order.TransitionTo(OrderStatus.Received);
        order.RecordActualQuantity(3m);
        order.TransitionTo(OrderStatus.AwaitingProcess);
        order.TransitionTo(OrderStatus.Processing);
        Assert.Throws<InvalidOperationException>(() => order.TransitionTo(OrderStatus.Cancelled));
    }

    private static Order NewOrder() => new(Guid.NewGuid(), "ORD-001", Guid.NewGuid(), new PriceSnapshot(PricingUnit.Kilogram, 5_000m, 3m, 0m, 0m), 2m);
}

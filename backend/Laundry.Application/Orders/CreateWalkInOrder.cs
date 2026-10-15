using Laundry.Domain.Orders;

namespace Laundry.Application.Orders;

public sealed record CreateWalkInOrder(Guid CustomerId, string OrderNumber, PricingUnit Unit, decimal UnitPrice, decimal MinimumWeightKg, decimal EstimatedQuantity, decimal ExpressFee = 0, decimal DeliveryFee = 0);

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken);
    Task<Order?> FindAsync(Guid id, CancellationToken cancellationToken);
}

public sealed class OrderService(IOrderRepository orders)
{
    public async Task<Order> CreateWalkInAsync(CreateWalkInOrder request, CancellationToken cancellationToken)
    {
        var price = new PriceSnapshot(request.Unit, request.UnitPrice, request.MinimumWeightKg, request.ExpressFee, request.DeliveryFee);
        var order = new Order(Guid.NewGuid(), request.OrderNumber, request.CustomerId, price, request.EstimatedQuantity);
        await orders.AddAsync(order, cancellationToken);
        return order;
    }
}

using System.Collections.Concurrent;
using Laundry.Application.Orders;
using Laundry.Domain.Orders;

namespace Laundry.Infrastructure.Orders;

/// <summary>Development-only repository. Replace with EF Core + Npgsql before release.</summary>
public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();
    public Task AddAsync(Order order, CancellationToken cancellationToken) { _orders[order.Id] = order; return Task.CompletedTask; }
    public Task<Order?> FindAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(_orders.GetValueOrDefault(id));
}

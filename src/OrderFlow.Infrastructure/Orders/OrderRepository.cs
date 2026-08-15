using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Orders;
using OrderFlow.Infrastructure._Shared;

namespace OrderFlow.Infrastructure.Orders;

public sealed class OrderRepository(OrderFlowDbContext dbContext) : IOrderRepository
{
    public async Task<Order?> GetTrackedByIdAsync(Guid id, Guid customerId, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customerId, cancellationToken);
    }

    public void Add(Order order)
    {
        dbContext.Orders.Add(order);
    }
}

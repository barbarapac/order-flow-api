using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Orders;
using OrderFlow.Infrastructure._Shared;

namespace OrderFlow.Infrastructure.Orders;

public sealed class OrderRepository(OrderFlowDbContext dbContext) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, Guid customerId, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customerId, cancellationToken);
    }

    public async Task<Order?> GetTrackedByIdAsync(Guid id, Guid customerId, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customerId, cancellationToken);
    }

    public async Task<(IReadOnlyCollection<Order> Items, int TotalCount)> GetPagedAsync(
        Guid customerId, OrderStatus? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Orders.AsNoTracking().Where(o => o.CustomerId == customerId);

        if (status is not null)
            query = query.Where(o => o.Status == status);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .AsSplitQuery()
            .OrderByDescending(o => o.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void Add(Order order)
    {
        dbContext.Orders.Add(order);
    }
}

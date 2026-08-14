namespace OrderFlow.Domain.Orders;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, Guid customerId, CancellationToken cancellationToken);

    Task<Order?> GetTrackedByIdAsync(Guid id, Guid customerId, CancellationToken cancellationToken);

    Task<(IReadOnlyCollection<Order> Items, int TotalCount)> GetPagedAsync(
        Guid customerId, OrderStatus? status, int page, int pageSize, CancellationToken cancellationToken);

    void Add(Order order);
}

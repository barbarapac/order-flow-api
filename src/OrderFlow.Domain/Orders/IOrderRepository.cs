namespace OrderFlow.Domain.Orders;

public interface IOrderRepository
{
    Task<Order?> GetTrackedByIdAsync(Guid id, Guid customerId, CancellationToken cancellationToken);
    void Add(Order order);
}

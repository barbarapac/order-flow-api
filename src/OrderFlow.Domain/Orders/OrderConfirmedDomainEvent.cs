using Mediator;

namespace OrderFlow.Domain.Orders;

public sealed record OrderConfirmedDomainEvent(Guid OrderId, IReadOnlyCollection<OrderStockAdjustment> Items) : INotification;

public sealed record OrderStockAdjustment(Guid ProductId, int Quantity);

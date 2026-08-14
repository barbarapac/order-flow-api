using Mediator;

namespace OrderFlow.Domain.Orders.Events;

public sealed record OrderConfirmed(Guid OrderId, IReadOnlyCollection<OrderStockAdjustment> Items) : INotification;

public sealed record OrderStockAdjustment(Guid ProductId, int Quantity);

using Mediator;

namespace OrderFlow.Domain.Orders.Events;

public sealed record OrderCanceled(Guid OrderId, IReadOnlyCollection<OrderStockAdjustment> Items) : INotification;

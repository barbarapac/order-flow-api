using Mediator;
using OrderFlow.Application._Shared;
using OrderFlow.Domain.Orders;
using OrderFlow.Domain.Products;

namespace OrderFlow.Application.Products.OrderConfirmed;

public sealed class OrderConfirmedEventHandler(
    IProductRepository productRepository,
    IDistributedLock distributedLock)
    : INotificationHandler<OrderConfirmedDomainEvent>
{
    public async ValueTask Handle(OrderConfirmedDomainEvent notification, CancellationToken cancellationToken)
    {
        var items = notification.Items.OrderBy(i => i.ProductId).ToList();
        var locks = new List<IAsyncDisposable>();
        
        try
        {
            foreach (var item in items)
            {
                locks.Add(await distributedLock.AcquireAsync($"product:{item.ProductId}:stock", cancellationToken));
            }

            foreach (var item in items)
            {
                var affectedRows = await productRepository.DecrementStockAsync(item.ProductId, item.Quantity, cancellationToken);

                if (affectedRows == 0)
                {
                    throw InsufficientStockException.For(item.ProductId);
                }
            }
        }
        finally
        {
            foreach (var @lock in locks)
                await @lock.DisposeAsync();
        }
    }
}

using Mediator;
using OrderFlow.Application._Shared;
using OrderFlow.Domain.Products;

namespace OrderFlow.Application.Products.OrderCanceled;

public sealed class OrderCanceledEventHandler(
    IProductRepository productRepository,
    IDistributedLock distributedLock)
    : INotificationHandler<Domain.Orders.Events.OrderCanceled>
{
    public async ValueTask Handle(Domain.Orders.Events.OrderCanceled notification, CancellationToken cancellationToken)
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
                await productRepository.IncrementStockAsync(item.ProductId, item.Quantity, cancellationToken);
            }
        }
        finally
        {
            foreach (var @lock in locks)
                await @lock.DisposeAsync();
        }
    }
}

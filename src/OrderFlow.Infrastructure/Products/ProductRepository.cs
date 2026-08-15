using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Products;
using OrderFlow.Infrastructure._Shared;

namespace OrderFlow.Infrastructure.Products;

public sealed class ProductRepository(OrderFlowDbContext dbContext) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Products.FindAsync([id], cancellationToken);
    }

    public void Add(Product product)
    {
        dbContext.Products.Add(product);
    }

    public void Remove(Product product)
    {
        dbContext.Products.Remove(product);
    }

    public async Task<int> DecrementStockAsync(Guid productId, int quantity, CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .Where(p => p.Id == productId && p.AvailableQuantity >= quantity)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(p => p.AvailableQuantity, p => p.AvailableQuantity - quantity),
                cancellationToken);
    }

    public async Task<int> IncrementStockAsync(Guid productId, int quantity, CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .Where(p => p.Id == productId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(p => p.AvailableQuantity, p => p.AvailableQuantity + quantity),
                cancellationToken);
    }
}

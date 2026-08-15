namespace OrderFlow.Domain.Products;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    void Add(Product product);
    void Remove(Product product);
    Task<int> DecrementStockAsync(Guid productId, int quantity, CancellationToken cancellationToken);
    Task<int> IncrementStockAsync(Guid productId, int quantity, CancellationToken cancellationToken);
}

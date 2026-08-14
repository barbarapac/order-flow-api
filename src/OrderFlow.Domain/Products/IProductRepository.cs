namespace OrderFlow.Domain.Products;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken);
    void Add(Product product);
    void Remove(Product product);
}

using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain._Shared;
using OrderFlow.Domain.Products;
using OrderFlow.Domain.Users;

namespace OrderFlow.Infrastructure._Shared;

public sealed class OrderFlowDbContext(DbContextOptions<OrderFlowDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderFlowDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

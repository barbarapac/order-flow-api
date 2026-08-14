using OrderFlow.Application._Shared;

namespace OrderFlow.Infrastructure._Shared;

public sealed class UnitOfWork(OrderFlowDbContext dbContext) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}

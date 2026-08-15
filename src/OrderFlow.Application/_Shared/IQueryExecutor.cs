namespace OrderFlow.Application._Shared;

public interface IQueryExecutor
{
    Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<T>> QueryAsync<T>(string sql, object? parameters, CancellationToken cancellationToken);
    Task<(int TotalCount, IReadOnlyCollection<T> Items)> QueryCountAndListAsync<T>(string sql, object? parameters, CancellationToken cancellationToken);
}

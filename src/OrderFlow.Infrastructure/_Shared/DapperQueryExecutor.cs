using System.Diagnostics.CodeAnalysis;
using Dapper;
using Npgsql;
using OrderFlow.Application._Shared;

namespace OrderFlow.Infrastructure._Shared;

[ExcludeFromCodeCoverage]
public sealed class DapperQueryExecutor(NpgsqlDataSource dataSource) : IQueryExecutor
{
    public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<T>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyCollection<T>> QueryAsync<T>(string sql, object? parameters, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var result = await connection.QueryAsync<T>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        return result.ToList();
    }

    public async Task<(int TotalCount, IReadOnlyCollection<T> Items)> QueryCountAndListAsync<T>(
        string sql, object? parameters, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        await using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        var totalCount = await multi.ReadSingleAsync<int>();
        var items = (await multi.ReadAsync<T>()).ToList();

        return (totalCount, items);
    }
}

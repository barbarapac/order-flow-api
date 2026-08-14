using Mediator;
using OrderFlow.Application._Shared;
using OrderFlow.Domain._Shared;

namespace OrderFlow.Application.Orders.GetById;

public sealed class GetOrderByIdQueryHandler(IQueryExecutor queryExecutor)
    : IQueryHandler<GetOrderByIdQuery, Result<GetOrderByIdResponse>>
{
    public async ValueTask<Result<GetOrderByIdResponse>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var header = await queryExecutor.QuerySingleOrDefaultAsync<GetOrderByIdResponse>(
            Sql.GetById, 
            new { Id = request.Id, request.CustomerId }, 
            cancellationToken);

        if (header is null)
        {
            return Result<GetOrderByIdResponse>.Failure(Error.NotFound("order.not_found", $"Pedido '{request.Id}' não encontrado."));
        }

        var rawItems = await queryExecutor.QueryAsync<GetOrderByIdItemResponse>(
            Sql.ItemsByOrderId,
            new { Id = request.Id }, 
            cancellationToken);

        var items = rawItems
            .Select(i => i with { LineTotal = i.UnitPrice * i.Quantity })
            .ToList();

        var response = header with { Total = items.Sum(i => i.LineTotal), Items = items };

        return Result<GetOrderByIdResponse>.Success(response);
    }
}

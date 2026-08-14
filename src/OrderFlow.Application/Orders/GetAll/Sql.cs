using Dapper;
using OrderFlow.Domain.Orders.Enums;

namespace OrderFlow.Application.Orders.GetAll;

internal static class Sql
{
    public static string CountAndPage(OrderStatus? status) => $"""
        SELECT COUNT(*)
        FROM orders
        WHERE customer_id = @CustomerId{StatusFilter(status)};

        SELECT "Id" AS Id, customer_id AS CustomerId, currency AS Currency, status AS Status,
               created_at_utc AS CreatedAtUtc, confirmed_at_utc AS ConfirmedAtUtc, canceled_at_utc AS CanceledAtUtc
        FROM orders
        WHERE customer_id = @CustomerId{StatusFilter(status)}
        ORDER BY created_at_utc DESC
        OFFSET @Skip LIMIT @PageSize;
        """;

    public const string ItemsByOrderIds = """
        SELECT order_id AS OrderId, product_id AS ProductId, unit_price AS UnitPrice, quantity AS Quantity
        FROM order_items
        WHERE order_id = ANY(@OrderIds)
        """;

    public static DynamicParameters Parameters(Guid customerId, OrderStatus? status, int skip, int pageSize)
    {
        var parameters = new DynamicParameters();
        parameters.Add("CustomerId", customerId);
        parameters.Add("Skip", skip);
        parameters.Add("PageSize", pageSize);

        if (status is not null)
            parameters.Add("Status", status.ToString());

        return parameters;
    }

    private static string StatusFilter(OrderStatus? status) =>
        status is null ? "" : "\n          AND status = @Status";
}

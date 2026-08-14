namespace OrderFlow.Application.Orders.GetById;

internal static class Sql
{
    public const string GetById = """
        SELECT "Id" AS Id, customer_id AS CustomerId, currency AS Currency, status AS Status,
               created_at_utc AS CreatedAtUtc, confirmed_at_utc AS ConfirmedAtUtc, canceled_at_utc AS CanceledAtUtc
        FROM orders
        WHERE "Id" = @Id AND customer_id = @CustomerId
        """;

    public const string ItemsByOrderId = """
        SELECT product_id AS ProductId, unit_price AS UnitPrice, quantity AS Quantity
        FROM order_items
        WHERE order_id = @Id
        """;
}

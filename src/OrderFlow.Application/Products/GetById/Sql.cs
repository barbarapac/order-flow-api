namespace OrderFlow.Application.Products.GetById;

internal static class Sql
{
    public const string GetById = """
        SELECT "Id" AS Id, name AS Name, unit_price AS UnitPrice,
               available_quantity AS AvailableQuantity, created_at_utc AS CreatedAtUtc
        FROM products
        WHERE "Id" = @Id
        """;
}

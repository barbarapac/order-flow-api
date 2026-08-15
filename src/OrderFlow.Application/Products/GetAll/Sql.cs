namespace OrderFlow.Application.Products.GetAll;

internal static class Sql
{
    public const string CountAndList = """
        SELECT COUNT(*) FROM products;

        SELECT "Id" AS Id, 
               name AS Name, 
               unit_price AS UnitPrice,
               available_quantity AS AvailableQuantity, 
               created_at_utc AS CreatedAtUtc
        FROM products
        ORDER BY created_at_utc DESC, "Id" DESC
        OFFSET @Skip LIMIT @PageSize;
        """;
}

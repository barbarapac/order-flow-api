using OrderFlow.Domain._Shared;

namespace OrderFlow.Application.Products.OrderConfirmed;

public sealed class InsufficientStockException : DomainException
{
    private InsufficientStockException(string message)
        : base("order.insufficient_stock", message, ErrorType.Conflict) { }

    public static InsufficientStockException For(Guid productId) =>
        new($"Estoque insuficiente para o produto '{productId}'.");
}

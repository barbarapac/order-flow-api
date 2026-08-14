using OrderFlow.Domain._Shared;
using OrderFlow.Domain.Orders.Enums;

namespace OrderFlow.Domain.Orders.Exceptions;

public sealed class OrderException : DomainException
{
    private OrderException(string code, string message, ErrorType type) : base(code, message, type) { }

    public static OrderException NoItems() =>
        new("order.no_items", "O pedido precisa ter ao menos um item.", ErrorType.Validation);

    public static OrderException InvalidQuantity(int quantity) =>
        new("order.invalid_quantity", $"A quantidade '{quantity}' deve ser maior que zero.", ErrorType.Validation);

    public static OrderException CurrencyRequired() =>
        new("order.invalid_currency", "A moeda não pode ser vazia.", ErrorType.Validation);

    public static OrderException CurrencyInvalidFormat(string rawValue) =>
        new("order.invalid_currency", $"'{rawValue}' não é um código de moeda ISO 4217 válido.", ErrorType.Validation);

    public static OrderException InvalidTransition(OrderStatus from, OrderStatus to) =>
        new("order.invalid_transition", $"Não é possível transicionar o pedido de '{from}' para '{to}'.", ErrorType.Conflict);
}

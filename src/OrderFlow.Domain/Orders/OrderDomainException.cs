using OrderFlow.Domain._Shared;

namespace OrderFlow.Domain.Orders;

public sealed class OrderDomainException : DomainException
{
    private OrderDomainException(string code, string message, ErrorType type) : base(code, message, type) { }

    public static OrderDomainException NoItems() =>
        new("order.no_items", "O pedido precisa ter ao menos um item.", ErrorType.Validation);

    public static OrderDomainException InvalidQuantity(int quantity) =>
        new("order.invalid_quantity", $"A quantidade '{quantity}' deve ser maior que zero.", ErrorType.Validation);

    public static OrderDomainException CurrencyRequired() =>
        new("order.invalid_currency", "A moeda não pode ser vazia.", ErrorType.Validation);

    public static OrderDomainException CurrencyInvalidFormat(string rawValue) =>
        new("order.invalid_currency", $"'{rawValue}' não é um código de moeda ISO 4217 válido.", ErrorType.Validation);
}

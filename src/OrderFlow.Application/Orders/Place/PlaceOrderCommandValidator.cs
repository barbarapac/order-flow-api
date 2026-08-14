using FluentValidation;

namespace OrderFlow.Application.Orders.Place;

public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("A moeda é obrigatória.")
            .Length(3).WithMessage("A moeda deve ter 3 letras (ISO 4217).");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("O pedido precisa ter ao menos um item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEqual(Guid.Empty).WithMessage("O produto do item é obrigatório.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("A quantidade do item deve ser maior que zero.");
        });
    }
}

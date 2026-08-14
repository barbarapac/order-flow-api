using FluentValidation;

namespace OrderFlow.Application.Products.Create;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome do produto é obrigatório.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("O preço unitário deve ser maior que zero.");

        RuleFor(x => x.AvailableQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("A quantidade disponível não pode ser negativa.");
    }
}

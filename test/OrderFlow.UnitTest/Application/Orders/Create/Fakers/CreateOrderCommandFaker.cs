using AutoBogus;
using OrderFlow.Application.Orders.Create;

namespace OrderFlow.UnitTest.Application.Orders.Create.Fakers;

public static class CreateOrderCommandFaker
{
    public static CreteOrderCommand Valid(IReadOnlyCollection<CreateOrderItemRequest> items)
    {
        return new AutoFaker<CreteOrderCommand>()
            .RuleFor(x => x.CustomerId, f => f.Random.Guid())
            .RuleFor(x => x.Currency, f => f.Finance.Currency().Code)
            .RuleFor(x => x.Items, _ => items)
            .Generate();
    }
}

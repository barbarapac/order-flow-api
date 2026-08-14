using AutoBogus;
using OrderFlow.Application.Orders.Place;

namespace OrderFlow.UnitTest.Application.Orders.Place.Fakers;

public static class PlaceOrderCommandFaker
{
    public static PlaceOrderCommand Valid(IReadOnlyCollection<PlaceOrderItemRequest> items)
    {
        return new AutoFaker<PlaceOrderCommand>()
            .RuleFor(x => x.CustomerId, f => f.Random.Guid())
            .RuleFor(x => x.Currency, f => f.Finance.Currency().Code)
            .RuleFor(x => x.Items, _ => items)
            .Generate();
    }
}

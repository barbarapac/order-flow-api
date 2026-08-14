using FluentAssertions;
using OrderFlow.Application.Orders.GetById;
using OrderFlow.Domain._Shared;
using OrderFlow.UnitTest.Application.Orders.GetById.Fakers;
using OrderFlow.UnitTest.Application.Orders.GetById.Fixtures;

namespace OrderFlow.UnitTest.Application.Orders.GetById;

public class GetOrderByIdQueryHandlerTests : GetOrderByIdQueryHandlerFixture
{
    [Fact]
    public async Task Handle_WithExistingOrder_ReturnsOrderWithComputedTotals()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var header = OrderFaker.ValidHeader(customerId);
        var items = OrderFaker.ValidItems();
        QueryExecutorMock.ConfigureGetByIdToReturn(header, items);

        // Act
        var result = await Handler.Handle(new GetOrderByIdQuery(header.Id, customerId), default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(header.Id);
        result.Value.CustomerId.Should().Be(customerId);
        result.Value.Currency.Should().Be(header.Currency);
        result.Value.Status.Should().Be(header.Status);
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.Single().ProductId.Should().Be(items[0].ProductId);
        result.Value.Items.Single().LineTotal.Should().Be(items[0].UnitPrice * items[0].Quantity);
        result.Value.Total.Should().Be(result.Value.Items.Sum(i => i.LineTotal));
    }

    [Fact]
    public async Task Handle_WithNonExistingOrder_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        QueryExecutorMock.ConfigureGetByIdToReturn(null);

        // Act
        var result = await Handler.Handle(new GetOrderByIdQuery(id, customerId), default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("order.not_found");
    }
}

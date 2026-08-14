using FluentAssertions;
using OrderFlow.Application.Orders.GetById;
using OrderFlow.Domain._Shared;
using OrderFlow.UnitTest.Application.Orders.GetById.Fakers;
using OrderFlow.UnitTest.Application.Orders.GetById.Fixtures;

namespace OrderFlow.UnitTest.Application.Orders.GetById;

public class GetOrderByIdQueryHandlerTests : GetOrderByIdQueryHandlerFixture
{
    [Fact]
    public async Task Handle_WithExistingOrder_ReturnsOrder()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var order = OrderFaker.Valid(customerId);
        OrderRepositoryMock.ConfigureGetByIdToReturn(order.Id, customerId, order);

        // Act
        var result = await Handler.Handle(new GetOrderByIdQuery(order.Id, customerId), default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(order.Id);
        result.Value.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public async Task Handle_WithNonExistingOrder_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        OrderRepositoryMock.ConfigureGetByIdToReturn(id, customerId, null);

        // Act
        var result = await Handler.Handle(new GetOrderByIdQuery(id, customerId), default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("order.not_found");
    }
}

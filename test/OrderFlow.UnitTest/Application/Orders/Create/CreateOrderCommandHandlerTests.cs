using FluentAssertions;
using OrderFlow.Application.Orders.Create;
using OrderFlow.Domain._Shared;
using OrderFlow.UnitTest.Application.Orders.Create.Fakers;
using OrderFlow.UnitTest.Application.Orders.Create.Fixtures;

namespace OrderFlow.UnitTest.Application.Orders.Create;

public class CreateOrderCommandHandlerTests : CreateOrderCommandHandlerFixture
{
    [Fact]
    public async Task Handle_WithValidCommand_CreateOrderAndPersists()
    {
        // Arrange
        var product = ProductFaker.Valid();
        ProductRepositoryMock.ConfigureGetByIdToReturn(product.Id, product);

        var items = new List<CreateOrderItemRequest> { new(product.Id, 2) };
        var command = CreateOrderCommandFaker.Valid(items);

        // Act
        var result = await Handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CustomerId.Should().Be(command.CustomerId);
        result.Value.Currency.Should().Be(command.Currency.ToUpperInvariant());
        result.Value.Status.Should().Be("Placed");
        result.Value.Total.Should().Be(product.UnitPrice * 2);
        result.Value.Items.Should().ContainSingle(i =>
            i.ProductId == product.Id && i.UnitPrice == product.UnitPrice && i.Quantity == 2);

        OrderRepositoryMock.VerifyAddWasCalledWith(command.CustomerId, command.Currency.ToUpperInvariant());
        UnitOfWorkMock.VerifySaveChangesWasCalled();
    }

    [Fact]
    public async Task Handle_WithNonExistingProduct_ReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        ProductRepositoryMock.ConfigureGetByIdToReturn(productId, null);

        var items = new List<CreateOrderItemRequest> { new(productId, 1) };
        var command = CreateOrderCommandFaker.Valid(items);

        // Act
        var result = await Handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("product.not_found");
    }

    [Fact]
    public async Task Handle_WithInsufficientStock_ReturnsConflict()
    {
        // Arrange
        var product = ProductFaker.Valid();
        product.AvailableQuantity = 1;
        ProductRepositoryMock.ConfigureGetByIdToReturn(product.Id, product);

        var items = new List<CreateOrderItemRequest> { new(product.Id, 5) };
        var command = CreateOrderCommandFaker.Valid(items);

        // Act
        var result = await Handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("order.insufficient_stock");
    }
}

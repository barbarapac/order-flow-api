using FluentAssertions;
using OrderFlow.Domain._Shared;
using OrderFlow.UnitTest.Application.Products.Update.Fakers;
using OrderFlow.UnitTest.Application.Products.Update.Fixtures;

namespace OrderFlow.UnitTest.Application.Products.Update;

public class UpdateProductCommandHandlerTests : UpdateProductCommandHandlerFixture
{
    [Fact]
    public async Task Handle_WithExistingProduct_UpdatesAndPersists()
    {
        // Arrange
        var product = ProductFaker.Valid();
        var command = UpdateProductCommandFaker.Valid() with { Id = product.Id };

        ProductRepositoryMock.ConfigureGetByIdToReturn(product.Id, product);

        // Act
        var result = await Handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(command.Name);
        result.Value.UnitPrice.Should().Be(command.UnitPrice);
        result.Value.AvailableQuantity.Should().Be(command.AvailableQuantity);

        product.Name.Should().Be(command.Name);
        product.UnitPrice.Should().Be(command.UnitPrice);
        product.AvailableQuantity.Should().Be(command.AvailableQuantity);

        UnitOfWorkMock.VerifySaveChangesWasCalled();
    }

    [Fact]
    public async Task Handle_WithNonExistingProduct_ReturnsNotFoundWithoutPersisting()
    {
        // Arrange
        var command = UpdateProductCommandFaker.Valid();
        ProductRepositoryMock.ConfigureGetByIdToReturn(command.Id, null);

        // Act
        var result = await Handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("product.not_found");

        UnitOfWorkMock.VerifySaveChangesWasNotCalled();
    }
}

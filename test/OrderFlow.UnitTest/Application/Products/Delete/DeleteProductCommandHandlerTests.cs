using FluentAssertions;
using OrderFlow.Application.Products.Delete;
using OrderFlow.Domain._Shared;
using OrderFlow.UnitTest.Application.Products.Delete.Fakers;
using OrderFlow.UnitTest.Application.Products.Delete.Fixtures;

namespace OrderFlow.UnitTest.Application.Products.Delete;

public class DeleteProductCommandHandlerTests : DeleteProductCommandHandlerFixture
{
    [Fact]
    public async Task Handle_WithExistingProduct_RemovesAndPersists()
    {
        // Arrange
        var product = ProductFaker.Valid();
        ProductRepositoryMock.ConfigureGetByIdToReturn(product.Id, product);

        // Act
        var result = await Handler.Handle(new DeleteProductCommand(product.Id), default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        ProductRepositoryMock.VerifyRemoveWasCalledWith(product);
        UnitOfWorkMock.VerifySaveChangesWasCalled();
    }

    [Fact]
    public async Task Handle_WithNonExistingProduct_ReturnsNotFoundWithoutPersisting()
    {
        // Arrange
        var id = Guid.NewGuid();
        ProductRepositoryMock.ConfigureGetByIdToReturn(id, null);

        // Act
        var result = await Handler.Handle(new DeleteProductCommand(id), default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("product.not_found");

        ProductRepositoryMock.VerifyRemoveWasNotCalled();
        UnitOfWorkMock.VerifySaveChangesWasNotCalled();
    }
}

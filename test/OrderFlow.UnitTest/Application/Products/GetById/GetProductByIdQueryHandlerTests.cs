using FluentAssertions;
using OrderFlow.Application.Products.GetById;
using OrderFlow.Domain._Shared;
using OrderFlow.UnitTest.Application.Products.GetById.Fakers;
using OrderFlow.UnitTest.Application.Products.GetById.Fixtures;

namespace OrderFlow.UnitTest.Application.Products.GetById;

public class GetProductByIdQueryHandlerTests : GetProductByIdQueryHandlerFixture
{
    [Fact]
    public async Task Handle_WithExistingProduct_ReturnsProduct()
    {
        // Arrange
        var product = ProductFaker.Valid();
        ProductRepositoryMock.ConfigureGetByIdToReturn(product.Id, product);

        // Act
        var result = await Handler.Handle(new GetProductByIdQuery(product.Id), default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(product.Id);
        result.Value.Name.Should().Be(product.Name);
    }

    [Fact]
    public async Task Handle_WithNonExistingProduct_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        ProductRepositoryMock.ConfigureGetByIdToReturn(id, null);

        // Act
        var result = await Handler.Handle(new GetProductByIdQuery(id), default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("product.not_found");
    }
}

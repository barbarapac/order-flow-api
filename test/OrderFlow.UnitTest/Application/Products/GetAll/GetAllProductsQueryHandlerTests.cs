using FluentAssertions;
using OrderFlow.Application.Products.GetAll;
using OrderFlow.UnitTest.Application.Products.GetAll.Fakers;
using OrderFlow.UnitTest.Application.Products.GetAll.Fixtures;

namespace OrderFlow.UnitTest.Application.Products.GetAll;

public class GetAllProductsQueryHandlerTests : GetAllProductsQueryHandlerFixture
{
    [Fact]
    public async Task Handle_WithExistingProducts_ReturnsAllMapped()
    {
        // Arrange
        var products = ProductFaker.ManyValid(3);
        ProductRepositoryMock.ConfigureGetAllToReturn(products);

        // Act
        var result = await Handler.Handle(new GetAllProductsQuery(), default);

        // Assert
        result.Should().HaveCount(3);
        result.Select(r => r.Id).Should().BeEquivalentTo(products.Select(p => p.Id));
    }

    [Fact]
    public async Task Handle_WithNoProducts_ReturnsEmptyCollection()
    {
        // Arrange
        ProductRepositoryMock.ConfigureGetAllToReturn([]);

        // Act
        var result = await Handler.Handle(new GetAllProductsQuery(), default);

        // Assert
        result.Should().BeEmpty();
    }
}

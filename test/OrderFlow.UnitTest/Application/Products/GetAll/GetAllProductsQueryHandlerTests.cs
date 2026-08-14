using FluentAssertions;
using OrderFlow.Application.Products.GetAll;
using OrderFlow.UnitTest.Application.Products.GetAll.Fakers;
using OrderFlow.UnitTest.Application.Products.GetAll.Fixtures;

namespace OrderFlow.UnitTest.Application.Products.GetAll;

public class GetAllProductsQueryHandlerTests : GetAllProductsQueryHandlerFixture
{
    [Fact]
    public async Task Handle_WithExistingProducts_ReturnsPagedMapped()
    {
        // Arrange
        var products = ProductFaker.ManyValid(3);
        QueryExecutorMock.ConfigureGetPagedToReturn(products, 3);

        // Act
        var result = await Handler.Handle(new GetAllProductsQuery(1, 20), default);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items.Should().BeEquivalentTo(products);
        result.TotalCount.Should().Be(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithNoProducts_ReturnsEmptyPage()
    {
        // Arrange
        QueryExecutorMock.ConfigureGetPagedToReturn([], 0);

        // Act
        var result = await Handler.Handle(new GetAllProductsQuery(1, 20), default);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}

using FluentAssertions;
using OrderFlow.Application.Orders.GetAll;
using OrderFlow.UnitTest.Application.Orders.GetAll.Fakers;
using OrderFlow.UnitTest.Application.Orders.GetAll.Fixtures;

namespace OrderFlow.UnitTest.Application.Orders.GetAll;

public class GetAllOrdersQueryHandlerTests : GetAllOrdersQueryHandlerFixture
{
    [Fact]
    public async Task Handle_WithExistingOrders_ReturnsPagedMapped()
    {
        // Arrange
        var orders = OrderFaker.ManyValid(3);
        OrderRepositoryMock.ConfigureGetPagedToReturn(orders, 3);

        // Act
        var result = await Handler.Handle(new GetAllOrdersQuery(Guid.NewGuid(), null, 1, 20), default);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items.Select(i => i.Id).Should().BeEquivalentTo(orders.Select(o => o.Id));
        result.TotalCount.Should().Be(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithNoOrders_ReturnsEmptyPage()
    {
        // Arrange
        OrderRepositoryMock.ConfigureGetPagedToReturn([], 0);

        // Act
        var result = await Handler.Handle(new GetAllOrdersQuery(Guid.NewGuid(), null, 1, 20), default);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}

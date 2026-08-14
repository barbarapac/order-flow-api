using FluentAssertions;
using OrderFlow.Application.Orders.GetAll;
using OrderFlow.UnitTest.Application.Orders.GetAll.Fakers;
using OrderFlow.UnitTest.Application.Orders.GetAll.Fixtures;

namespace OrderFlow.UnitTest.Application.Orders.GetAll;

public class GetAllOrdersQueryHandlerTests : GetAllOrdersQueryHandlerFixture
{
    [Fact]
    public async Task Handle_WithExistingOrders_ReturnsPagedWithComputedTotals()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var headers = OrderFaker.ManyValidHeaders(2, customerId);
        var itemRows = headers.Select(h => OrderFaker.ItemRowFor(h.Id)).ToList();
        QueryExecutorMock.ConfigureGetPagedToReturn(headers, 2, itemRows);

        // Act
        var result = await Handler.Handle(new GetAllOrdersQuery(customerId, null, 1, 20), default);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Select(i => i.Id).Should().BeEquivalentTo(headers.Select(h => h.Id));
        result.Items.Should().OnlyContain(i => i.Items.Count == 1);
        result.Items.Should().OnlyContain(i => i.Total == i.Items.Sum(x => x.LineTotal));
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithNoOrders_ReturnsEmptyPage()
    {
        // Arrange
        QueryExecutorMock.ConfigureGetPagedToReturn([], 0);

        // Act
        var result = await Handler.Handle(new GetAllOrdersQuery(Guid.NewGuid(), null, 1, 20), default);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}

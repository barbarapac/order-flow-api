using Moq;
using OrderFlow.Application._Shared;
using OrderFlow.Application.Orders.GetAll;

namespace OrderFlow.UnitTest.Application.Orders.GetAll.Mocks;

public class QueryExecutorMock : Mock<IQueryExecutor>
{
    public void ConfigureGetPagedToReturn(
        IReadOnlyCollection<GetAllOrdersResponse> headers, int totalCount, IReadOnlyCollection<OrderItemRow>? itemRows = null)
    {
        Setup(x => x.QueryCountAndListAsync<GetAllOrdersResponse>(
                It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((totalCount, headers));

        Setup(x => x.QueryAsync<OrderItemRow>(
                It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(itemRows ?? []);
    }
}

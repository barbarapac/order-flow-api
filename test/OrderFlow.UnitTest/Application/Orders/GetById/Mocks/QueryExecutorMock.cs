using Moq;
using OrderFlow.Application._Shared;
using OrderFlow.Application.Orders.GetById;

namespace OrderFlow.UnitTest.Application.Orders.GetById.Mocks;

public class QueryExecutorMock : Mock<IQueryExecutor>
{
    public void ConfigureGetByIdToReturn(GetOrderByIdResponse? header, IReadOnlyCollection<GetOrderByIdItemResponse>? items = null)
    {
        Setup(x => x.QuerySingleOrDefaultAsync<GetOrderByIdResponse>(
                It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(header);

        Setup(x => x.QueryAsync<GetOrderByIdItemResponse>(
                It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(items ?? []);
    }
}

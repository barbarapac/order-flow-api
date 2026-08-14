using Moq;
using OrderFlow.Application._Shared;
using OrderFlow.Application.Products.GetAll;

namespace OrderFlow.UnitTest.Application.Products.GetAll.Mocks;

public class QueryExecutorMock : Mock<IQueryExecutor>
{
    public void ConfigureGetPagedToReturn(IReadOnlyCollection<GetAllProductsResponse> items, int totalCount)
    {
        Setup(x => x.QueryCountAndListAsync<GetAllProductsResponse>(
                It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((totalCount, items));
    }
}

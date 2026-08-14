using Moq;
using OrderFlow.Application._Shared;
using OrderFlow.Application.Products.GetById;

namespace OrderFlow.UnitTest.Application.Products.GetById.Mocks;

public class QueryExecutorMock : Mock<IQueryExecutor>
{
    public void ConfigureGetByIdToReturn(GetProductByIdResponse? product)
    {
        Setup(x => x.QuerySingleOrDefaultAsync<GetProductByIdResponse>(
                It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
    }
}

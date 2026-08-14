using Moq;
using OrderFlow.Application._Shared;

namespace OrderFlow.UnitTest.Application.Orders.Place.Mocks;

public class UnitOfWorkMock : Mock<IUnitOfWork>
{
    public void VerifySaveChangesWasCalled()
    {
        Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

using Moq;
using OrderFlow.Application._Shared;

namespace OrderFlow.UnitTest.Application.Products.Update.Mocks;

public class UnitOfWorkMock : Mock<IUnitOfWork>
{
    public void VerifySaveChangesWasCalled()
    {
        Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    public void VerifySaveChangesWasNotCalled()
    {
        Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

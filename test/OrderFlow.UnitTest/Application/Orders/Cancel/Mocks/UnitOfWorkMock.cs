using Moq;
using OrderFlow.Application._Shared;

namespace OrderFlow.UnitTest.Application.Orders.Cancel.Mocks;

public class UnitOfWorkMock : Mock<IUnitOfWork>
{
    public void VerifyTransactionWasCommitted()
    {
        Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    public void VerifyTransactionWasRolledBack()
    {
        Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    public void VerifyNoTransactionWasOpened()
    {
        Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

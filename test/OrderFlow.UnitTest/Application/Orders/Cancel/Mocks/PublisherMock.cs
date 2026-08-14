using Mediator;
using Moq;

namespace OrderFlow.UnitTest.Application.Orders.Cancel.Mocks;

public class PublisherMock : Mock<IPublisher>
{
    public void VerifyPublishWasCalledOnce()
    {
        Verify(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public void VerifyPublishWasNeverCalled()
    {
        Verify(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

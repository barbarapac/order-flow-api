using Moq;
using OrderFlow.Application._Shared;

namespace OrderFlow.UnitTest.Application.Orders.Confirm.Mocks;

public class DistributedLockMock : Mock<IDistributedLock>
{
    public List<string> AcquiredResources { get; } = [];
    public List<string> ReleasedResources { get; } = [];

    public DistributedLockMock()
    {
        Setup(l => l.AcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns((string resource, CancellationToken _) =>
            {
                AcquiredResources.Add(resource);
                return Task.FromResult<IAsyncDisposable>(new FakeLockHandle(() => ReleasedResources.Add(resource)));
            });
    }

    private sealed class FakeLockHandle(Action onDispose) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            onDispose();
            return ValueTask.CompletedTask;
        }
    }
}

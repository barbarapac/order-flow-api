namespace OrderFlow.Application._Shared;

public interface IDistributedLock
{
    Task<IAsyncDisposable> AcquireAsync(string resource, CancellationToken cancellationToken = default);
}

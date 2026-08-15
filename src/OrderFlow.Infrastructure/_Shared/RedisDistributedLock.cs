using System.Diagnostics.CodeAnalysis;
using OrderFlow.Application._Shared;
using StackExchange.Redis;

namespace OrderFlow.Infrastructure._Shared;

[ExcludeFromCodeCoverage]
public sealed class RedisDistributedLock(IConnectionMultiplexer redis) : IDistributedLock
{
    private static readonly TimeSpan LockExpiry = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan PollingDelay = TimeSpan.FromMilliseconds(50);

    // Script Lua porque GET+DEL via dois comandos separados não é atômico: entre a leitura e o
    // delete o lock pode expirar e ser readquirido por outro processo, e um DEL incondicional
    // apagaria o lock alheio. O EVAL roda no Redis (single-threaded) como uma única operação.
    private const string ReleaseScript = """
        if redis.call('GET', KEYS[1]) == ARGV[1] then
            return redis.call('DEL', KEYS[1])
        end
        return 0
        """;

    public async Task<IAsyncDisposable> AcquireAsync(string resource, CancellationToken cancellationToken = default)
    {
        var database = redis.GetDatabase();
        var key = $"lock:{resource}";
        var value = Guid.NewGuid().ToString();

        while (!await database.StringSetAsync(key, value, LockExpiry, When.NotExists))
        {
            await Task.Delay(PollingDelay, cancellationToken);
        }

        return new LockHandle(database, key, value);
    }

    private sealed class LockHandle(IDatabase database, string key, string value) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await database.ScriptEvaluateAsync(ReleaseScript, [key], [value]);
        }
    }
}

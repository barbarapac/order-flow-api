using FluentAssertions;
using Moq;
using OrderFlow.Infrastructure._Shared;
using StackExchange.Redis;

namespace OrderFlow.UnitTest.Infrastructure._Shared;

public class RedisDistributedLockTests
{
    private readonly Mock<IDatabase> _database = new();
    private readonly Mock<IConnectionMultiplexer> _connectionMultiplexer = new();
    private readonly RedisDistributedLock _sut;

    public RedisDistributedLockTests()
    {
        _connectionMultiplexer
            .Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_database.Object);

        _sut = new RedisDistributedLock(_connectionMultiplexer.Object);
    }

    [Fact]
    public async Task AcquireAsync_WhenKeyIsFree_AcquiresOnFirstAttempt()
    {
        // Arrange
        SetupStringSet(true);

        // Act
        var handle = await _sut.AcquireAsync("stock:product-1");

        // Assert
        handle.Should().NotBeNull();
        _database.Verify(d => d.StringSetAsync(
            It.Is<RedisKey>(k => k == "lock:stock:product-1"),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            When.NotExists), Times.Once);
    }

    [Fact]
    public async Task AcquireAsync_WhenKeyIsHeld_PollsUntilAcquired()
    {
        // Arrange
        _database
            .SetupSequence(d => d.StringSetAsync(
                It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<When>()))
            .ReturnsAsync(false)
            .ReturnsAsync(false)
            .ReturnsAsync(true);

        // Act
        var handle = await _sut.AcquireAsync("stock:product-1");

        // Assert
        handle.Should().NotBeNull();
        _database.Verify(d => d.StringSetAsync(
            It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<When>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task AcquireAsync_WhenCancelledWhilePolling_ThrowsOperationCanceledException()
    {
        // Arrange
        SetupStringSet(false);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = () => _sut.AcquireAsync("stock:product-1", cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task DisposeAsync_ReleasesLockViaAtomicScript()
    {
        // Arrange
        SetupStringSet(true);
        SetupScriptEvaluate();
        var handle = await _sut.AcquireAsync("stock:product-1");

        // Act
        await handle.DisposeAsync();

        // Assert
        _database.Verify(d => d.ScriptEvaluateAsync(
            It.IsAny<string>(),
            It.Is<RedisKey[]>(keys => keys.Length == 1 && keys[0] == "lock:stock:product-1"),
            It.Is<RedisValue[]>(values => values.Length == 1),
            It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task DisposeAsync_WhenScriptReportsValueMismatch_CompletesWithoutThrowing()
    {
        // Arrange: simula o lock ja expirado e readquirido por outro processo — o script
        // Lua retorna 0 (no-op) em vez de deletar. A logica condicional de GET/DEL roda
        // dentro do Redis e nao pode ser exercida pelo mock; este teste so garante que o
        // lado C# nao adiciona nenhuma checagem extra sobre o retorno do script.
        SetupStringSet(true);
        _database
            .Setup(d => d.ScriptEvaluateAsync(
                It.IsAny<string>(), It.IsAny<RedisKey[]>(), It.IsAny<RedisValue[]>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisResult.Create(0L));
        var handle = await _sut.AcquireAsync("stock:product-1");

        // Act
        var act = async () => await handle.DisposeAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    private void SetupStringSet(bool result)
    {
        _database
            .Setup(d => d.StringSetAsync(
                It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<When>()))
            .ReturnsAsync(result);
    }

    private void SetupScriptEvaluate()
    {
        _database
            .Setup(d => d.ScriptEvaluateAsync(
                It.IsAny<string>(), It.IsAny<RedisKey[]>(), It.IsAny<RedisValue[]>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisResult.Create(1L));
    }
}

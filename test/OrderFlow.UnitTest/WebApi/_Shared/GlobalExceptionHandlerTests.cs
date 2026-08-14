using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderFlow.Domain.Orders.Enums;
using OrderFlow.Domain.Orders.Exceptions;
using OrderFlow.WebApi._Shared;

namespace OrderFlow.UnitTest.WebApi._Shared;

public class GlobalExceptionHandlerTests
{
    private readonly FakeLogger<GlobalExceptionHandler> _logger = new();
    private readonly GlobalExceptionHandler _sut;

    public GlobalExceptionHandlerTests()
    {
        _sut = new GlobalExceptionHandler(_logger);
    }

    [Fact]
    public async Task TryHandleAsync_WhenClientDisconnects_ReturnsHandledWithoutTouchingResponseOrLoggingAsError()
    {
        // Arrange: exceção de cancelamento cujo token é o mesmo do RequestAborted — simula o
        // cliente fechando a conexão no meio de um Confirm/Cancel em transação.
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var httpContext = new DefaultHttpContext { RequestAborted = cts.Token };
        // Nenhum IProblemDetailsService registrado de propósito: se o handler não fizer o
        // short-circuit e cair no caminho normal, GetRequiredService lança e o teste falha alto.
        httpContext.RequestServices = new ServiceCollection().BuildServiceProvider();

        var exception = new OperationCanceledException("client disconnected", cts.Token);

        // Act
        var handled = await _sut.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        handled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK); // não tocado

        _logger.Entries.Should().ContainSingle(e => e.Level == LogLevel.Information);
        _logger.Entries.Should().NotContain(e => e.Level == LogLevel.Error || e.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task TryHandleAsync_WhenOperationCanceledButRequestWasNotAborted_FallsBackToUnexpectedErrorHandling()
    {
        // Arrange: OperationCanceledException que não está ligada ao cancelamento da requisição
        // (ex.: um timeout interno qualquer) — não deve ser tratada como "cliente foi embora".
        var httpContext = new DefaultHttpContext { RequestServices = BuildProblemDetailsServiceProvider() };
        httpContext.Response.Body = new MemoryStream();

        var exception = new OperationCanceledException("unrelated timeout");

        // Act
        var handled = await _sut.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        handled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        _logger.Entries.Should().ContainSingle(e => e.Level == LogLevel.Error);
    }

    [Fact]
    public async Task TryHandleAsync_WithDomainException_MapsErrorTypeToStatusCodeAndLogsWarning()
    {
        // Arrange
        var httpContext = new DefaultHttpContext { RequestServices = BuildProblemDetailsServiceProvider() };
        httpContext.Response.Body = new MemoryStream();

        var exception = OrderException.InvalidTransition(OrderStatus.Canceled, OrderStatus.Confirmed);

        // Act
        var handled = await _sut.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        handled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        _logger.Entries.Should().ContainSingle(e => e.Level == LogLevel.Warning);
    }

    private static IServiceProvider BuildProblemDetailsServiceProvider() =>
        new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider();

    private sealed class FakeLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception)));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}

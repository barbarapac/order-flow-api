using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Domain._Shared;
using OrderFlow.WebApi._Shared;

namespace OrderFlow.UnitTest.WebApi._Shared;

public class ResultExtensionsTests
{
    [Fact]
    public void ToProblemResult_CarriesTheStatusTitleAndErrorCode()
    {
        // Arrange
        var error = Error.NotFound("order.not_found", "Order not found.");

        // Act
        var result = error.ToProblemResult();

        // Assert
        var problem = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problem.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        problem.ProblemDetails.Title.Should().Be("Order not found.");
        problem.ProblemDetails.Extensions["errorCode"].Should().Be("order.not_found");
    }

    [Fact]
    public async Task ToProblemResult_WritesAProblemDetailsPayloadWithTheErrorCodeExtension()
    {
        // Arrange
        var error = Error.BusinessRule("order.invalid_transition", "Cannot confirm a canceled order.");

        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider()
        };
        httpContext.Response.Body = new MemoryStream();

        // Act
        await error.ToProblemResult().ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);

        httpContext.Response.Body.Position = 0;
        using var payload = await JsonDocument.ParseAsync(httpContext.Response.Body);

        payload.RootElement.GetProperty("title").GetString().Should().Be("Cannot confirm a canceled order.");
        payload.RootElement.GetProperty("errorCode").GetString().Should().Be("order.invalid_transition");
    }
}

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using OrderFlow.Domain._Shared;
using OrderFlow.WebApi._Shared;

namespace OrderFlow.UnitTest.WebApi._Shared;

public class ErrorTypeExtensionsTests
{
    [Theory]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.Unauthorized, StatusCodes.Status401Unauthorized)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.BusinessRule, StatusCodes.Status422UnprocessableEntity)]
    public void ToStatusCode_MapsEachKnownErrorType(ErrorType type, int expectedStatusCode)
    {
        // Act
        var statusCode = type.ToStatusCode();

        // Assert
        statusCode.Should().Be(expectedStatusCode);
    }

    [Fact]
    public void ToStatusCode_ForAnUnmappedErrorType_FallsBackTo500()
    {
        // Arrange
        var unknownType = (ErrorType)999;

        // Act
        var statusCode = unknownType.ToStatusCode();

        // Assert
        statusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}

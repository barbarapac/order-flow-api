using FluentAssertions;
using OrderFlow.Domain._Shared;

namespace OrderFlow.UnitTest.Domain._Shared;

public class ResultTests
{
    [Fact]
    public void Success_ExposesTheValueAndNoError()
    {
        // Act
        var result = Result<string>.Success("ok");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
        result.Value.Should().Be("ok");
    }

    [Fact]
    public void Failure_ExposesTheError()
    {
        // Arrange
        var error = Error.NotFound("order.not_found", "Order not found.");

        // Act
        var result = Result<string>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Value_OnAFailedResult_Throws()
    {
        // Arrange: os endpoints leem result.Value só depois de checar IsSuccess. Se alguém
        // inverter a ordem, o erro tem de aparecer aqui e não virar um null silencioso na resposta.
        var result = Result<string>.Failure(Error.NotFound("order.not_found", "Order not found."));

        // Act
        var act = () => result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot access the value of a failed result.");
    }

    [Fact]
    public void NonGenericSuccess_HasNoErrorAndNoValue()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        result.Value.Should().BeNull();
    }

    [Fact]
    public void NonGenericFailure_ExposesTheErrorAndBlocksTheValue()
    {
        // Arrange
        var error = Error.NotFound("product.not_found", "Product not found.");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);

        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>();
    }
}

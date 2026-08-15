using FluentAssertions;
using OrderFlow.Domain._Shared;

namespace OrderFlow.UnitTest.Domain._Shared;

public class ErrorTests
{
    // As factories são o único jeito de criar um Error, e é o Type escolhido aqui que decide o
    // status HTTP lá na borda — trocar um por outro muda a resposta da API sem quebrar nada.
    [Fact]
    public void NotFound_CarriesTheCodeMessageAndNotFoundType()
    {
        // Act
        var error = Error.NotFound("order.not_found", "Order not found.");

        // Assert
        error.Code.Should().Be("order.not_found");
        error.Message.Should().Be("Order not found.");
        error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Validation_UsesTheValidationType()
    {
        // Act & Assert
        Error.Validation("order.invalid_page", "Page must be positive.")
            .Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Conflict_UsesTheConflictType()
    {
        // Act & Assert
        Error.Conflict("user.email_already_used", "E-mail already registered.")
            .Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void BusinessRule_UsesTheBusinessRuleType()
    {
        // Act & Assert
        Error.BusinessRule("order.invalid_transition", "Cannot confirm a canceled order.")
            .Type.Should().Be(ErrorType.BusinessRule);
    }

    [Fact]
    public void Unauthorized_UsesTheUnauthorizedType()
    {
        // Act & Assert
        Error.Unauthorized("auth.invalid_credentials", "Invalid credentials.")
            .Type.Should().Be(ErrorType.Unauthorized);
    }
}

using FluentAssertions;
using OrderFlow.Application.Products.Update;

namespace OrderFlow.UnitTest.Application.Products.Update;

public class UpdateProductCommandValidatorTests
{
    private readonly UpdateProductCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_IsValid()
    {
        // Arrange
        var command = new UpdateProductCommand(Guid.NewGuid(), "Notebook", 1999.90m, 10);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyId_HasErrorForId()
    {
        // Arrange
        var command = new UpdateProductCommand(Guid.Empty, "Notebook", 1999.90m, 10);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(UpdateProductCommand.Id) &&
            e.ErrorMessage == "O id do produto é obrigatório.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyName_HasErrorForName(string name)
    {
        // Arrange
        var command = new UpdateProductCommand(Guid.NewGuid(), name, 1999.90m, 10);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(UpdateProductCommand.Name) &&
            e.ErrorMessage == "O nome do produto é obrigatório.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithNonPositiveUnitPrice_HasErrorForUnitPrice(decimal unitPrice)
    {
        // Arrange
        var command = new UpdateProductCommand(Guid.NewGuid(), "Notebook", unitPrice, 10);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(UpdateProductCommand.UnitPrice) &&
            e.ErrorMessage == "O preço unitário deve ser maior que zero.");
    }

    [Fact]
    public void Validate_WithNegativeAvailableQuantity_HasErrorForAvailableQuantity()
    {
        // Arrange
        var command = new UpdateProductCommand(Guid.NewGuid(), "Notebook", 1999.90m, -1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(UpdateProductCommand.AvailableQuantity) &&
            e.ErrorMessage == "A quantidade disponível não pode ser negativa.");
    }
}

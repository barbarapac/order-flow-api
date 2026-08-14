using FluentAssertions;
using OrderFlow.Application.Products.Create;

namespace OrderFlow.UnitTest.Application.Products.Create;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_IsValid()
    {
        // Arrange
        var command = new CreateProductCommand("Notebook", 1999.90m, 10);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyName_HasErrorForName(string name)
    {
        // Arrange
        var command = new CreateProductCommand(name, 1999.90m, 10);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateProductCommand.Name) &&
            e.ErrorMessage == "O nome do produto é obrigatório.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithNonPositiveUnitPrice_HasErrorForUnitPrice(decimal unitPrice)
    {
        // Arrange
        var command = new CreateProductCommand("Notebook", unitPrice, 10);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateProductCommand.UnitPrice) &&
            e.ErrorMessage == "O preço unitário deve ser maior que zero.");
    }

    [Fact]
    public void Validate_WithNegativeAvailableQuantity_HasErrorForAvailableQuantity()
    {
        // Arrange
        var command = new CreateProductCommand("Notebook", 1999.90m, -1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateProductCommand.AvailableQuantity) &&
            e.ErrorMessage == "A quantidade disponível não pode ser negativa.");
    }

    [Fact]
    public void Validate_WithZeroAvailableQuantity_IsValid()
    {
        // Arrange
        var command = new CreateProductCommand("Notebook", 1999.90m, 0);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

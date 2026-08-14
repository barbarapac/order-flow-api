using FluentAssertions;
using OrderFlow.Application.Products.GetAll;

namespace OrderFlow.UnitTest.Application.Products.GetAll;

public class GetAllProductsQueryValidatorTests
{
    private readonly GetAllProductsQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidQuery_IsValid()
    {
        // Arrange
        var query = new GetAllProductsQuery(1, 20);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidPage_HasErrorForPage(int page)
    {
        // Arrange
        var query = new GetAllProductsQuery(page, 20);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllProductsQuery.Page));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_WithInvalidPageSize_HasErrorForPageSize(int pageSize)
    {
        // Arrange
        var query = new GetAllProductsQuery(1, pageSize);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllProductsQuery.PageSize));
    }
}

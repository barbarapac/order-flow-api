using FluentAssertions;
using OrderFlow.Application.Orders.GetAll;

namespace OrderFlow.UnitTest.Application.Orders.GetAll;

public class GetAllOrdersQueryValidatorTests
{
    private readonly GetAllOrdersQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidQuery_IsValid()
    {
        // Arrange
        var query = new GetAllOrdersQuery(Guid.NewGuid(), null, 1, 20);

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
        var query = new GetAllOrdersQuery(Guid.NewGuid(), null, page, 20);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllOrdersQuery.Page));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_WithInvalidPageSize_HasErrorForPageSize(int pageSize)
    {
        // Arrange
        var query = new GetAllOrdersQuery(Guid.NewGuid(), null, 1, pageSize);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllOrdersQuery.PageSize));
    }
}

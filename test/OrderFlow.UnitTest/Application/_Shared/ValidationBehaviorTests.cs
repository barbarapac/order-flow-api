using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using OrderFlow.Application._Shared;

namespace OrderFlow.UnitTest.Application._Shared;

public class ValidationBehaviorTests
{
    public sealed record FakeRequest;

    public sealed record FakeResponse;

    private readonly FakeRequest _request = new();
    private readonly FakeResponse _expectedResponse = new();

    [Fact]
    public async Task Handle_WithNoValidators_CallsNextAndReturnsResponse()
    {
        // Arrange
        var behavior = new ValidationBehavior<FakeRequest, FakeResponse>([]);
        var nextCalled = false;

        // Act
        var response = await behavior.Handle(_request, Next, default);

        // Assert
        response.Should().Be(_expectedResponse);
        nextCalled.Should().BeTrue();
        return;

        Task<FakeResponse> Next(CancellationToken _)
        {
            nextCalled = true;
            return Task.FromResult(_expectedResponse);
        }
    }

    [Fact]
    public async Task Handle_WithAllValidatorsPassing_CallsNextAndReturnsResponse()
    {
        // Arrange
        var validator = new Mock<IValidator<FakeRequest>>();
        validator.Setup(v => v.ValidateAsync(_request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<FakeRequest, FakeResponse>([validator.Object]);
        var nextCalled = false;

        // Act
        var response = await behavior.Handle(_request, Next, default);

        // Assert
        response.Should().Be(_expectedResponse);
        nextCalled.Should().BeTrue();
        return;

        Task<FakeResponse> Next(CancellationToken _)
        {
            nextCalled = true;
            return Task.FromResult(_expectedResponse);
        }
    }

    [Fact]
    public async Task Handle_WithFailingValidator_ThrowsValidationExceptionAndDoesNotCallNext()
    {
        // Arrange
        var failure = new ValidationFailure("Email", "Email is required.");
        var validator = new Mock<IValidator<FakeRequest>>();
        validator.Setup(v => v.ValidateAsync(_request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([failure]));

        var behavior = new ValidationBehavior<FakeRequest, FakeResponse>([validator.Object]);
        var nextCalled = false;

        // Act
        var act = async () => await behavior.Handle(_request, Next, default);

        // Assert
        (await act.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().ContainSingle(e => e.PropertyName == "Email");
        nextCalled.Should().BeFalse();
        return;

        Task<FakeResponse> Next(CancellationToken _)
        {
            nextCalled = true;
            return Task.FromResult(_expectedResponse);
        }
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_AggregatesFailuresFromAll()
    {
        // Arrange
        var firstValidator = new Mock<IValidator<FakeRequest>>();
        firstValidator.Setup(v => v.ValidateAsync(_request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Email", "Email is required.")]));

        var secondValidator = new Mock<IValidator<FakeRequest>>();
        secondValidator.Setup(v => v.ValidateAsync(_request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Password", "Password is required.")]));

        var behavior = new ValidationBehavior<FakeRequest, FakeResponse>([firstValidator.Object, secondValidator.Object]);

        // Act
        var act = async () => await behavior.Handle(_request, _ => Task.FromResult(_expectedResponse), default);

        // Assert
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().Contain(e => e.PropertyName == "Email");
        exception.Which.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}

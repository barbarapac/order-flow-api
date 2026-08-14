using FluentAssertions;
using OrderFlow.UnitTest.Application.Products.Create.Fakers;
using OrderFlow.UnitTest.Application.Products.Create.Fixtures;

namespace OrderFlow.UnitTest.Application.Products.Create;

public class CreateProductCommandHandlerTests : CreateProductCommandHandlerFixture
{
    [Fact]
    public async Task Handle_WithValidCommand_CreatesProductAndPersists()
    {
        // Arrange
        var command = CreateProductCommandFaker.Valid();

        // Act
        var result = await Handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(command.Name);
        result.Value.UnitPrice.Should().Be(command.UnitPrice);
        result.Value.AvailableQuantity.Should().Be(command.AvailableQuantity);
        result.Value.Id.Should().NotBeEmpty();

        ProductRepositoryMock.VerifyAddWasCalledWith(command.Name, command.UnitPrice, command.AvailableQuantity);
        UnitOfWorkMock.VerifySaveChangesWasCalled();
    }
}

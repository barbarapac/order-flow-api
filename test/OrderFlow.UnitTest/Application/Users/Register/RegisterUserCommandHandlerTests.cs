using FluentAssertions;
using OrderFlow.Domain._Shared;
using OrderFlow.Domain.Users.Exceptions;
using OrderFlow.UnitTest.Application.Users.Register.Fakers;
using OrderFlow.UnitTest.Application.Users.Register.Fixtures;

namespace OrderFlow.UnitTest.Application.Users.Register;

public class RegisterUserCommandHandlerTests : RegisterUserCommandHandlerFixture
{
    [Fact]
    public async Task Handle_WithNewEmail_RegistersUserAndPersists()
    {
        // Arrange
        var command = RegisterUserCommandFaker.Valid();

        UserRepositoryMock.ConfigureExistsByEmailToReturn(false);
        PasswordHasherMock.ConfigureHashToReturn("hashed-password");

        // Act
        var result = await Handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(command.Name);
        result.Value.Email.Should().Be(command.Email.ToLowerInvariant());

        UserRepositoryMock.VerifyAddWasCalledWith(command.Email.ToLowerInvariant());
        UnitOfWorkMock.VerifySaveChangesWasCalled();
    }

    [Fact]
    public async Task Handle_WithNewEmail_PersistsUserWithHashedPassword()
    {
        // Arrange
        var command = RegisterUserCommandFaker.Valid();

        UserRepositoryMock.ConfigureExistsByEmailToReturn(false);
        PasswordHasherMock.ConfigureHashToReturn("hashed-password");

        // Act
        await Handler.Handle(command, default);

        // Assert
        UserRepositoryMock.VerifyAddWasCalledWithPasswordHash("hashed-password");
    }

    [Fact]
    public async Task Handle_WithInvalidName_PropagatesDomainExceptionWithoutPersisting()
    {
        // Arrange
        var command = RegisterUserCommandFaker.Valid() with { Name = "   " };

        UserRepositoryMock.ConfigureExistsByEmailToReturn(false);
        PasswordHasherMock.ConfigureHashToReturn("hashed-password");

        // Act
        var act = async () => await Handler.Handle(command, default);

        // Assert
        (await act.Should().ThrowAsync<UserException>())
            .Which.Code.Should().Be("user.invalid_name");

        UserRepositoryMock.VerifyAddWasNotCalled();
        UnitOfWorkMock.VerifySaveChangesWasNotCalled();
    }

    [Fact]
    public async Task Handle_WithAlreadyRegisteredEmail_ReturnsConflictWithoutPersisting()
    {
        // Arrange
        var command = RegisterUserCommandFaker.Valid();
        UserRepositoryMock.ConfigureExistsByEmailToReturn(true);

        // Act
        var result = await Handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("user.email_already_registered");

        UserRepositoryMock.VerifyAddWasNotCalled();
        UnitOfWorkMock.VerifySaveChangesWasNotCalled();
    }
}

using OrderFlow.Application.Users.Register;
using OrderFlow.UnitTest.Application.Users.Register.Mocks;

namespace OrderFlow.UnitTest.Application.Users.Register.Fixtures;

public class RegisterUserCommandHandlerFixture
{
    protected UserRepositoryMock UserRepositoryMock { get; private set; }
    protected PasswordHasherMock PasswordHasherMock { get; private set; }
    protected UnitOfWorkMock UnitOfWorkMock { get; private set; }

    protected RegisterUserCommandHandler Handler { get; private set; }

    protected RegisterUserCommandHandlerFixture()
    {
        UserRepositoryMock = new UserRepositoryMock();
        PasswordHasherMock = new PasswordHasherMock();
        UnitOfWorkMock = new UnitOfWorkMock();

        Handler = new RegisterUserCommandHandler(
            UserRepositoryMock.Object,
            PasswordHasherMock.Object,
            UnitOfWorkMock.Object);
    }
}

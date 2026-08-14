using OrderFlow.Application.Auth.Login;
using OrderFlow.UnitTest.Application.Auth.Login.Mocks;

namespace OrderFlow.UnitTest.Application.Auth.Login.Fixtures;

public class LoginCommandHandlerFixture
{
    protected UserRepositoryMock UserRepositoryMock { get; private set; }
    protected PasswordHasherMock PasswordHasherMock { get; private set; }
    protected JwtTokenGeneratorMock JwtTokenGeneratorMock { get; private set; }

    protected LoginCommandHandler Handler { get; private set; }

    protected LoginCommandHandlerFixture()
    {
        UserRepositoryMock = new UserRepositoryMock();
        PasswordHasherMock = new PasswordHasherMock();
        JwtTokenGeneratorMock = new JwtTokenGeneratorMock();

        Handler = new LoginCommandHandler(
            UserRepositoryMock.Object,
            PasswordHasherMock.Object,
            JwtTokenGeneratorMock.Object);
    }
}

using Moq;
using OrderFlow.Application._Shared;
using OrderFlow.Domain.Users;

namespace OrderFlow.UnitTest.Application.Auth.Login.Mocks;

public class JwtTokenGeneratorMock : Mock<IJwtTokenGenerator>
{
    public void ConfigureGenerateToReturn(string token, DateTime expiresAtUtc)
    {
        Setup(g => g.Generate(It.IsAny<User>()))
            .Returns((token, expiresAtUtc));
    }

    public void VerifyGenerateWasNotCalled()
    {
        Verify(g => g.Generate(It.IsAny<User>()), Times.Never);
    }
}

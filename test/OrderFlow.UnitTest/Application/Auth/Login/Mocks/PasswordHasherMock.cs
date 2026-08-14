using Moq;
using OrderFlow.Application._Shared;

namespace OrderFlow.UnitTest.Application.Auth.Login.Mocks;

public class PasswordHasherMock : Mock<IPasswordHasher>
{
    public void ConfigureVerifyToReturn(bool result)
    {
        Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(result);
    }
}

using Moq;
using OrderFlow.Application._Shared;

namespace OrderFlow.UnitTest.Application.Users.Register.Mocks;

public class PasswordHasherMock : Mock<IPasswordHasher>
{
    public void ConfigureHashToReturn(string passwordHash)
    {
        Setup(h => h.Hash(It.IsAny<string>()))
            .Returns(passwordHash);
    }
}

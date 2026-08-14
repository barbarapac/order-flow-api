using Moq;
using OrderFlow.Domain.Users;

namespace OrderFlow.UnitTest.Application.Auth.Login.Mocks;

public class UserRepositoryMock : Mock<IUserRepository>
{
    public void ConfigureGetByEmailToReturn(User? user)
    {
        Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
    }

    public void VerifyGetByEmailWasCalledWith(string email)
    {
        Verify(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
    }
}

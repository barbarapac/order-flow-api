using Moq;
using OrderFlow.Domain.Users;

namespace OrderFlow.UnitTest.Application.Users.Register.Mocks;

public class UserRepositoryMock : Mock<IUserRepository>
{
    public void ConfigureExistsByEmailToReturn(bool exists)
    {
        Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    public void VerifyAddWasCalledWith(string email)
    {
        Verify(r => r.Add(It.Is<User>(u => u.Email.Value == email)), Times.Once);
    }

    public void VerifyAddWasCalledWithPasswordHash(string passwordHash)
    {
        Verify(r => r.Add(It.Is<User>(u => u.PasswordHash == passwordHash)), Times.Once);
    }

    public void VerifyAddWasNotCalled()
    {
        Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }
}

using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Users;
using OrderFlow.Infrastructure._Shared;

namespace OrderFlow.Infrastructure.Users;

public sealed class UserRepository(OrderFlowDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = Email.Create(email);
        return dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = Email.Create(email);
        return dbContext.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
    }

    public void Add(User user)
    {
        dbContext.Users.Add(user);
    }
}

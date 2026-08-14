using OrderFlow.Domain.Users;

namespace OrderFlow.Application._Shared;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) Generate(User user);
}

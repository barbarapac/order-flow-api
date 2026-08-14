using OrderFlow.Infrastructure.Auth;

namespace OrderFlow.UnitTest.Infrastructure.Security.Fakers;

public static class JwtOptionsFaker
{
    public static JwtOptions Valid()
    {
        return new JwtOptions
        {
            Issuer = "OrderFlow.Tests",
            Audience = "OrderFlow.Tests.Client",
            SigningKey = "unit-test-signing-key-at-least-32-bytes-long!!",
            ExpirationMinutes = 60
        };
    }
}

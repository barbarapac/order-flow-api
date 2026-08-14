using AutoBogus;
using OrderFlow.Application.Auth.Login;

namespace OrderFlow.UnitTest.Application.Auth.Login.Fakers;

public static class LoginCommandFaker
{
    public static LoginCommand Valid()
    {
        return new AutoFaker<LoginCommand>().Generate();
    }

    public static LoginCommand For(string email, string password)
    {
        return new AutoFaker<LoginCommand>()
            .RuleFor(x => x.Email, _ => email)
            .RuleFor(x => x.Password, _ => password)
            .Generate();
    }
}

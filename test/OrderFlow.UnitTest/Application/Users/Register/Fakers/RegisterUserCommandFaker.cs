using AutoBogus;
using OrderFlow.Application.Users.Register;

namespace OrderFlow.UnitTest.Application.Users.Register.Fakers;

public static class RegisterUserCommandFaker
{
    public static RegisterUserCommand Valid()
    {
        return new AutoFaker<RegisterUserCommand>()
            .RuleFor(x => x.Name, f => f.Person.FullName)
            .RuleFor(x => x.Email, f => f.Internet.Email())
            .RuleFor(x => x.Password, f => f.Internet.Password())
            .Generate();
    }

    public static RegisterUserCommand WithEmail(string email)
    {
        var command = Valid();
        return command with { Email = email };
    }
}

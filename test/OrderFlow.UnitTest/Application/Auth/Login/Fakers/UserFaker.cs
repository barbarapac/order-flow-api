using Bogus;
using OrderFlow.Domain.Users;

namespace OrderFlow.UnitTest.Application.Auth.Login.Fakers;

public static class UserFaker
{
    public static User Valid()
    {
        var faker = new Faker();

        return User.Register(faker.Person.FullName, faker.Internet.Email(), faker.Random.Hash());
    }

    public static User WithPasswordHash(string passwordHash)
    {
        var faker = new Faker();

        return User.Register(faker.Person.FullName, faker.Internet.Email(), passwordHash);
    }
}

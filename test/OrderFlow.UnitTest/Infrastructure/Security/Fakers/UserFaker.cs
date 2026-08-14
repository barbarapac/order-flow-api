using Bogus;
using OrderFlow.Domain.Users;

namespace OrderFlow.UnitTest.Infrastructure.Security.Fakers;

public static class UserFaker
{
    public static User Valid()
    {
        var faker = new Faker();

        return User.Register(faker.Person.FullName, faker.Internet.Email(), faker.Random.Hash());
    }
}

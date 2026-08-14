using Microsoft.Extensions.Options;
using OrderFlow.Infrastructure.Auth;
using OrderFlow.UnitTest.Infrastructure.Security.Fakers;

namespace OrderFlow.UnitTest.Infrastructure.Security.Fixtures;

public class JwtTokenGeneratorFixture
{
    protected JwtOptions Settings { get; private set; }
    protected JwtTokenGenerator Generator { get; private set; }

    protected JwtTokenGeneratorFixture()
    {
        Settings = JwtOptionsFaker.Valid();
        Generator = new JwtTokenGenerator(Options.Create(Settings));
    }
}

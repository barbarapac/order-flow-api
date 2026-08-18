using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using OrderFlow.ArchitectureTest.Fixtures;
using OrderFlow.WebApi._Shared;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace OrderFlow.ArchitectureTest;

public class LayerDependencyTests : ArchitectureFixture
{
    [Fact]
    public void Domain_AnyType_DoesNotDependOnOuterLayers()
    {
        // Arrange
        IArchRule rule = Types().That().Are(DomainLayer)
            .Should().NotDependOnAny(ApplicationLayer)
            .AndShould().NotDependOnAny(InfrastructureLayer)
            .AndShould().NotDependOnAny(WebApiLayer);

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void Domain_AnyType_DoesNotDependOnPersistenceOrWebFrameworks()
    {
        // Arrange
        IArchRule rule = Types().That().Are(DomainLayer)
            .Should().NotDependOnAnyTypesThat().ResideInNamespaceMatching(@"^Microsoft\.EntityFrameworkCore.*")
            .AndShould().NotDependOnAnyTypesThat().ResideInNamespaceMatching(@"^Microsoft\.AspNetCore.*")
            .AndShould().NotDependOnAnyTypesThat().ResideInNamespaceMatching(@"^Npgsql.*")
            .AndShould().NotDependOnAnyTypesThat().ResideInNamespaceMatching(@"^StackExchange\.Redis.*")
            .AndShould().NotDependOnAnyTypesThat().ResideInNamespaceMatching(@"^Dapper.*")
            .AndShould().NotDependOnAnyTypesThat().ResideInNamespaceMatching(@"^FluentValidation.*");

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void Application_AnyType_DoesNotDependOnOuterLayers()
    {
        // Arrange
        IArchRule rule = Types().That().Are(ApplicationLayer)
            .Should().NotDependOnAny(InfrastructureLayer)
            .AndShould().NotDependOnAny(WebApiLayer);

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void Application_AnyType_DoesNotDependOnPersistenceOrWebFrameworks()
    {
        // Arrange
        // Dapper é exceção consciente: o SQL literal das Queries mora na Application (ADR-014).
        IArchRule rule = Types().That().Are(ApplicationLayer)
            .Should().NotDependOnAnyTypesThat().ResideInNamespaceMatching(@"^Microsoft\.EntityFrameworkCore.*")
            .AndShould().NotDependOnAnyTypesThat().ResideInNamespaceMatching(@"^Microsoft\.AspNetCore.*")
            .AndShould().NotDependOnAnyTypesThat().ResideInNamespaceMatching(@"^Npgsql.*")
            .AndShould().NotDependOnAnyTypesThat().ResideInNamespaceMatching(@"^StackExchange\.Redis.*");

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void Infrastructure_AnyType_DoesNotDependOnWebApi()
    {
        // Arrange
        IArchRule rule = Types().That().Are(InfrastructureLayer)
            .Should().NotDependOnAny(WebApiLayer);

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void Endpoints_AnyImplementation_DoNotDependOnInfrastructure()
    {
        // Arrange
        // Program.cs e IoC.cs precisam da Infrastructure para o DI; os endpoints, não.
        IArchRule rule = Classes().That().ImplementInterface(typeof(IEndpoint))
            .Should().NotDependOnAny(InfrastructureLayer);

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void Endpoints_AnyImplementation_DoNotDependOnRepositories()
    {
        // Arrange
        // O endpoint só traduz HTTP em Command/Query: acesso a dados passa pela Application.
        IArchRule rule = Classes().That().ImplementInterface(typeof(IEndpoint))
            .Should().NotDependOnAnyTypesThat().HaveNameEndingWith("Repository");

        // Act & Assert
        rule.Check(Architecture);
    }
}

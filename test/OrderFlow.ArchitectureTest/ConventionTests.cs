using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using FluentValidation;
using Mediator;
using OrderFlow.Application.Products.OrderConfirmed;
using OrderFlow.ArchitectureTest.Fixtures;
using OrderFlow.Domain._Shared;
using OrderFlow.WebApi._Shared;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace OrderFlow.ArchitectureTest;

public class ConventionTests : ArchitectureFixture
{
    [Fact]
    public void Commands_AnyImplementation_AreSealedRecordsNamedCommand()
    {
        // Arrange
        IArchRule rule = Classes().That().ImplementInterface(typeof(ICommand<>))
            .Should().BeSealed()
            .AndShould().BeRecord()
            .AndShould().HaveNameEndingWith("Command")
            .AndShould().ResideInAssembly(ApplicationAssembly);

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void Commands_AnyTypeNamedCommand_ImplementsTheMediatorContract()
    {
        // Arrange
        IArchRule rule = Classes().That().HaveNameEndingWith("Command").And().ResideInAssembly(ApplicationAssembly)
            .Should().ImplementInterface(typeof(ICommand<>));

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void Queries_AnyImplementation_AreSealedRecordsNamedQuery()
    {
        // Arrange
        IArchRule rule = Classes().That().ImplementInterface(typeof(IQuery<>))
            .Should().BeSealed()
            .AndShould().BeRecord()
            .AndShould().HaveNameEndingWith("Query")
            .AndShould().ResideInAssembly(ApplicationAssembly);

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void Queries_AnyTypeNamedQuery_ImplementsTheMediatorContract()
    {
        // Arrange
        IArchRule rule = Classes().That().HaveNameEndingWith("Query").And().ResideInAssembly(ApplicationAssembly)
            .Should().ImplementInterface(typeof(IQuery<>));

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void CommandHandlers_AnyImplementation_AreSealedAndNamedHandler()
    {
        // Arrange
        IArchRule rule = Classes().That().ImplementInterface(typeof(ICommandHandler<,>))
            .Should().BeSealed()
            .AndShould().HaveNameEndingWith("CommandHandler")
            .AndShould().ResideInAssembly(ApplicationAssembly);

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void QueryHandlers_AnyImplementation_AreSealedAndNamedHandler()
    {
        // Arrange
        IArchRule rule = Classes().That().ImplementInterface(typeof(IQueryHandler<,>))
            .Should().BeSealed()
            .AndShould().HaveNameEndingWith("QueryHandler")
            .AndShould().ResideInAssembly(ApplicationAssembly);

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void EventHandlers_AnyImplementation_AreSealedAndNamedEventHandler()
    {
        // Arrange
        IArchRule rule = Classes().That().ImplementInterface(typeof(INotificationHandler<>))
            .Should().BeSealed()
            .AndShould().HaveNameEndingWith("EventHandler")
            .AndShould().ResideInAssembly(ApplicationAssembly);

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void Validators_AnyImplementation_AreSealedAndNamedValidator()
    {
        // Arrange
        IArchRule rule = Classes().That().AreAssignableTo(typeof(IValidator)).And().ResideInAssembly(ApplicationAssembly)
            .Should().BeSealed()
            .AndShould().HaveNameEndingWith("Validator");

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void Endpoints_AnyImplementation_AreSealedAndNamedEndpoint()
    {
        // Arrange
        IArchRule rule = Classes().That().ImplementInterface(typeof(IEndpoint))
            .Should().BeSealed()
            .AndShould().HaveNameEndingWith("Endpoint")
            .AndShould().ResideInAssembly(WebApiAssembly);

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void Endpoints_AnyTypeNamedEndpoint_ImplementsIEndpoint()
    {
        // Arrange
        // A descoberta é por reflection sobre IEndpoint: um endpoint sem a interface não é
        // registrado e some da API em silêncio.
        IArchRule rule = Classes().That().HaveNameEndingWith("Endpoint").And().ResideInAssembly(WebApiAssembly)
            .Should().ImplementInterface(typeof(IEndpoint));

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void Guards_InDomain_AreInternalStaticClasses()
    {
        // Arrange
        // Classe estática, em IL, é abstract + sealed.
        IArchRule rule = Classes().That().HaveNameEndingWith("Guard").And().ResideInAssembly(DomainAssembly)
            .Should().BeInternal()
            .AndShould().BeAbstract()
            .AndShould().BeSealed();

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void DomainExceptions_AnySubclass_AreSealedAndNamedException()
    {
        // Arrange
        IArchRule rule = Classes().That().AreAssignableTo(typeof(DomainException)).And().AreNot(typeof(DomainException))
            .Should().BeSealed()
            .AndShould().HaveNameEndingWith("Exception");

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void Application_AnyExceptionType_IsOnlyTheInsufficientStockOne()
    {
        // Arrange
        // Falha esperada na Application é Result<T>.Failure, não exception (docs/error-handling.md).
        // A única exception que nasce ali é a do estoque insuficiente, que derruba a transação da
        // confirmação de propósito (ADR-006).
        IArchRule rule = Classes().That().ResideInAssembly(ApplicationAssembly).And().AreAssignableTo(typeof(Exception))
            .Should().Be(typeof(InsufficientStockException));

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void SqlClasses_InApplication_AreInternalStaticClasses()
    {
        // Arrange
        IArchRule rule = Classes().That().HaveName("Sql").And().ResideInAssembly(ApplicationAssembly)
            .Should().BeInternal()
            .AndShould().BeAbstract()
            .AndShould().BeSealed();

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void Repositories_InInfrastructure_AreSealedAndImplementTheDomainInterface()
    {
        // Arrange
        IArchRule rule = Classes().That().HaveNameEndingWith("Repository").And().ResideInAssembly(InfrastructureAssembly)
            .Should().BeSealed()
            .AndShould().ImplementAnyInterfacesThat().ResideInAssembly(DomainAssembly);

        // Act & Assert
        rule.Check(Architecture);
    }
}

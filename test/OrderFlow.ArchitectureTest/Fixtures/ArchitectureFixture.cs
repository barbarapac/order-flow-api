using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using OrderFlow.Application.Orders.Confirm;
using OrderFlow.Domain.Orders;
using OrderFlow.Infrastructure.Orders;
using OrderFlow.WebApi._Shared;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Assembly = System.Reflection.Assembly;

namespace OrderFlow.ArchitectureTest.Fixtures;

public abstract class ArchitectureFixture
{
    protected static readonly Assembly DomainAssembly         = typeof(Order).Assembly;
    protected static readonly Assembly ApplicationAssembly    = typeof(ConfirmOrderCommand).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(OrderRepository).Assembly;
    protected static readonly Assembly WebApiAssembly         = typeof(IEndpoint).Assembly;

    protected static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(DomainAssembly, ApplicationAssembly, InfrastructureAssembly, WebApiAssembly)
        .Build();

    protected static readonly IObjectProvider<IType> DomainLayer =
        Types().That().ResideInAssembly(DomainAssembly).As("Domain");

    protected static readonly IObjectProvider<IType> ApplicationLayer =
        Types().That().ResideInAssembly(ApplicationAssembly).As("Application");

    protected static readonly IObjectProvider<IType> InfrastructureLayer =
        Types().That().ResideInAssembly(InfrastructureAssembly).As("Infrastructure");

    protected static readonly IObjectProvider<IType> WebApiLayer =
        Types().That().ResideInAssembly(WebApiAssembly).As("WebApi");
}

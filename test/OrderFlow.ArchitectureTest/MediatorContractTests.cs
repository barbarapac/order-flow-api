using FluentAssertions;
using Mediator;
using OrderFlow.ArchitectureTest.Fixtures;

namespace OrderFlow.ArchitectureTest;

public class MediatorContractTests : ArchitectureFixture
{
    private static readonly Type[] RequestContracts = [typeof(ICommand<>), typeof(IQuery<>)];

    private static readonly Type[] HandlerContracts = [typeof(ICommandHandler<,>), typeof(IQueryHandler<,>)];

    [Fact]
    public void CommandsAndQueries_EveryRequestType_HasExactlyOneHandler()
    {
        // Arrange
        var requests = ApplicationAssembly.GetTypes()
            .Where(type => Implements(type, RequestContracts))
            .ToArray();

        var handledRequests = ApplicationAssembly.GetTypes()
            .SelectMany(type => type.GetInterfaces())
            .Where(contract => contract.IsGenericType && HandlerContracts.Contains(contract.GetGenericTypeDefinition()))
            .Select(contract => contract.GetGenericArguments()[0])
            .ToArray();

        // Act
        var requestsWithoutSingleHandler = requests
            .Where(request => handledRequests.Count(handled => handled == request) != 1)
            .Select(request => request.Name)
            .ToArray();

        // Assert
        requests.Should().NotBeEmpty();
        requestsWithoutSingleHandler.Should().BeEmpty("todo Command/Query precisa de exatamente um handler");
    }

    private static bool Implements(Type type, Type[] contracts) =>
        type.GetInterfaces().Any(contract =>
            contract.IsGenericType && contracts.Contains(contract.GetGenericTypeDefinition()));
}

using System.Text.RegularExpressions;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using FluentAssertions;
using OrderFlow.ArchitectureTest.Fixtures;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Assembly = System.Reflection.Assembly;

namespace OrderFlow.ArchitectureTest;

public class VerticalSliceTests : ArchitectureFixture
{
    private static readonly string[] ApplicationSliceNamespaces = SlicesOf(ApplicationAssembly, "OrderFlow.Application");
    private static readonly string[] WebApiSliceNamespaces      = SlicesOf(WebApiAssembly, "OrderFlow.WebApi");

    public static TheoryData<string> ApplicationSlices => ToTheoryData(ApplicationSliceNamespaces);

    public static TheoryData<string> WebApiSlices => ToTheoryData(WebApiSliceNamespaces);

    [Fact]
    public void SliceDiscovery_ForBothLayers_FindsFeatureNamespaces()
    {
        // Assert
        // Guarda contra a descoberta silenciosamente vazia: sem slices, as Theories abaixo não
        // avaliariam nada e passariam sem verificar coisa alguma.
        ApplicationSliceNamespaces.Should().NotBeEmpty();
        WebApiSliceNamespaces.Should().NotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(ApplicationSlices))]
    public void ApplicationSlice_AnyType_DoesNotDependOnAnotherSlice(string slice)
    {
        // Arrange
        IArchRule rule = Types().That().ResideInNamespaceMatching(NamespacePattern(slice))
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespaceMatching(SiblingSlicesPattern(slice, ApplicationSliceNamespaces));

        // Act & Assert
        rule.Check(Architecture);
    }

    [Theory]
    [MemberData(nameof(WebApiSlices))]
    public void WebApiSlice_AnyType_DoesNotDependOnAnotherSlice(string slice)
    {
        // Arrange
        IArchRule rule = Types().That().ResideInNamespaceMatching(NamespacePattern(slice))
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespaceMatching(SiblingSlicesPattern(slice, WebApiSliceNamespaces));

        // Act & Assert
        rule.Check(Architecture);
    }

    private static string[] SlicesOf(Assembly assembly, string root)
    {
        return assembly.GetTypes()
            .Select(type => type.Namespace)
            .Where(@namespace => @namespace is not null && @namespace.StartsWith($"{root}.", StringComparison.Ordinal))
            .Select(@namespace => @namespace!.Split('.'))
            .Where(segments => segments.Length >= 4)
            .Select(segments => string.Join('.', segments.Take(4)))
            .Where(@namespace => !@namespace.Contains("._Shared", StringComparison.Ordinal))
            .Distinct()
            .Order()
            .ToArray();
    }

    private static TheoryData<string> ToTheoryData(string[] slices)
    {
        var data = new TheoryData<string>();

        foreach (var slice in slices)
        {
            data.Add(slice);
        }

        return data;
    }

    private static string NamespacePattern(string @namespace) => $@"^{Regex.Escape(@namespace)}(\..*)?$";

    private static string SiblingSlicesPattern(string slice, string[] allSlices)
    {
        var siblings = allSlices
            .Where(other => other != slice)
            .Select(Regex.Escape);

        return $@"^({string.Join('|', siblings)})(\..*)?$";
    }
}

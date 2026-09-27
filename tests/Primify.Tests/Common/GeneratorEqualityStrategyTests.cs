namespace Primify.Generator.Tests.Common;

/// <summary>
/// Pins the generator decisions that are derived from symbols rather than from how a declaration
/// happens to be spelled: the equality strategy and the set of using directives in generated files.
/// </summary>
public class GeneratorEqualityStrategyTests
{
    [Test]
    [Arguments("string", "string.Equals(Value, other.Value, System.StringComparison.Ordinal)")]
    [Arguments("System.String", "string.Equals(Value, other.Value, System.StringComparison.Ordinal)")]
    [Arguments("int", "Value == other.Value")]
    [Arguments("System.Guid", "Value == other.Value")]
    [Arguments("System.DateTimeOffset", "Value == other.Value")]
    [Arguments("System.TimeSpan", "Value == other.Value")]
    [Arguments("nint", "Value == other.Value")]
    [Arguments("double", "System.Collections.Generic.EqualityComparer<double>.Default.Equals(Value, other.Value)")]
    [Arguments("float", "System.Collections.Generic.EqualityComparer<float>.Default.Equals(Value, other.Value)")]
    [Arguments("System.Half", "System.Collections.Generic.EqualityComparer<System.Half>.Default.Equals(Value, other.Value)")]
    public async Task Strategy_DependsOnSymbol_NotSpelling(string wrappedType, string expectedEquality)
    {
        var (diagnostics, generated) = GeneratorSnapshotTests.Generate($$"""
            using Primify.Attributes;

            namespace Snap;

            [Primify<{{wrappedType}}>]
            public partial struct Strategy;
            """);

        await Assert.That(diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error))
            .IsEmpty();
        await Assert.That(generated).Contains(expectedEquality);
    }

    [Test]
    public async Task CustomStruct_FallsBackToEqualityComparer()
    {
        const string code = """
            using Primify.Attributes;

            namespace Snap;

            public readonly record struct Money(long Cents);

            [Primify<Money>]
            public partial struct Price;
            """;

        var (diagnostics, generated) = GeneratorSnapshotTests.Generate(code);

        await Assert.That(diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error))
            .IsEmpty();
        await Assert.That(generated).Contains(
            "System.Collections.Generic.EqualityComparer<Snap.Money>.Default.Equals(Value, other.Value)");
    }

    [Test]
    [Arguments("string")]
    [Arguments("int")]
    [Arguments("double")]
    public async Task Generated_Source_Omits_Unused_Usings(string wrappedType)
    {
        var (_, generated) = GeneratorSnapshotTests.Generate($$"""
            using Primify.Attributes;

            namespace Snap;

            [Primify<{{wrappedType}}>]
            public partial struct UsingShape;
            """);

        // The module initializer spells its attribute out in full, so the namespace import is dead weight
        // that would surface as CS8019 in every consuming project.
        await Assert.That(generated).DoesNotContain("using System.Runtime.CompilerServices;");
        await Assert.That(generated).Contains("using Primify.Converters;");
    }
}

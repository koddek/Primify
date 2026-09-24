namespace Primify.Generator.Tests.Common;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

public class GeneratorCompilationTests
{
    [Test]
    public async Task SupportedWrapper_Compiles()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Snap;

                            [Primify<int>]
                            public readonly partial record struct Wrapper;
                            """;

        var (diagnostics, generated) = GeneratorSnapshotTests.Generate(code);

        await AssertNoErrors(diagnostics);
        await Assert.That(generated).IsNotNull();
    }

    [Test]
    public async Task GlobalNamespace_Compiles()
    {
        const string code = """
                            using Primify.Attributes;

                            [Primify<int>]
                            public readonly partial record struct GlobalWrapper;
                            """;

        var (diagnostics, generated) = GeneratorSnapshotTests.Generate(code);

        await AssertNoErrors(diagnostics);
        await Assert.That(generated).DoesNotContain("namespace <global namespace>");
    }

    [Test]
    public async Task BareRecord_Compiles()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Snap;

                            [Primify<int>]
                            public partial record BareRecordWrapper;
                            """;

        var (diagnostics, generated) = GeneratorSnapshotTests.Generate(code);

        await AssertNoErrors(diagnostics);
        await Assert.That(generated).Contains("partial record class BareRecordWrapper");
    }

    [Test]
    public async Task MultiplePartials_EmitOnce()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Snap;

                            [Primify<int>]
                            public partial record struct SplitWrapper;

                            public partial record struct SplitWrapper
                            {
                            }
                            """;

        var (diagnostics, generated) = GeneratorSnapshotTests.Generate(code);

        await AssertNoErrors(diagnostics);
        await Assert.That(generated).Contains("partial record struct SplitWrapper");
    }

    [Test]
    public async Task NonMatchingConstructor_IsAllowed()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Snap;

                            [Primify.Attributes.Primify<int>]
                            public partial class CustomConstructor
                            {
                                public CustomConstructor(string value) { }
                            }
                            """;

        var (diagnostics, generated) = GeneratorSnapshotTests.Generate(code);

        await AssertNoErrors(diagnostics);
        await Assert.That(generated).IsNotNull();
    }

    [Test]
    public async Task AliasedAttribute_Compiles()
    {
        const string code = """
                            using P = Primify.Attributes.PrimifyAttribute<int>;

                            namespace Snap;

                            [P]
                            public partial record struct AliasedWrapper;
                            """;

        var (diagnostics, generated) = GeneratorSnapshotTests.Generate(code);

        await AssertNoErrors(diagnostics);
        await Assert.That(generated).IsNotNull();
    }

    private static async Task AssertNoErrors(ImmutableArray<Diagnostic> diagnostics)
    {
        var errors = diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

        await Assert.That(errors).IsEmpty();
    }
}

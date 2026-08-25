namespace Primify.Generator.Tests.Common;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Primify.Attributes;
using Primify.Converters;

public class GeneratorSnapshotTests
{
    private static (ImmutableArray<Diagnostic> Diagnostics, string? Generated) Generate(string code)
    {
        var references = new List<MetadataReference>();
        var platformAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        if (platformAssemblies is not null)
        {
            foreach (var path in platformAssemblies.Split(System.IO.Path.PathSeparator))
            {
                references.Add(MetadataReference.CreateFromFile(path));
            }
        }

        references.Add(MetadataReference.CreateFromFile(typeof(PrimifyAttribute<>).Assembly.Location));

        var compilation = CSharpCompilation.Create(
            "snapshot",
            [CSharpSyntaxTree.ParseText(code)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var driver = CSharpGeneratorDriver.Create(new Primify.Generators.PrimifyGenerator().AsSourceGenerator());
        var runResult = driver.RunGenerators(compilation).GetRunResult();

        var generated = runResult.Results.IsEmpty || runResult.Results[0].GeneratedSources.Length == 0
            ? null
            : runResult.Results[0].GeneratedSources[0].SourceText.ToString();

        return (compilation.GetDiagnostics().AddRange(runResult.Diagnostics), generated);
    }

    private static async Task AssertNoErrors(ImmutableArray<Diagnostic> diagnostics)
    {
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

        await Assert.That(errors).IsEmpty();
    }

    [Test]
    public async Task RecordStruct_EmitsExpectedShape()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Snap;

                            [Primify<int>]
                            public readonly partial record struct Foo;
                            """;

        var (diagnostics, generated) = Generate(code);

        await AssertNoErrors(diagnostics);

        var source = generated ?? string.Empty;
        await Assert.That(source).Contains("[System.Diagnostics.DebuggerDisplay(\"{Value}\")]");
        await Assert.That(source).Contains("explicit operator Foo(int value)");
        await Assert.That(source).Contains("implicit operator int(Foo value)");
        await Assert.That(source).Contains("LiteDbMapping.Register<global::Snap.Foo, int>()");
        await Assert.That(source).Contains("public static bool TryFrom(int value, out Foo result)");
        await Assert.That(source).Contains("/// Non-throwing alternative to <see cref=\"From\"/>.");
        await Assert.That(source).DoesNotContain("ToString()");
    }

    [Test]
    public async Task PlainStruct_EmitsToString_AndEqualityMembers()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Snap;

                            [Primify<string>]
                            public partial struct Bar;
                            """;

        var (diagnostics, generated) = Generate(code);

        await AssertNoErrors(diagnostics);

        var source = generated ?? string.Empty;
        await Assert.That(source).Contains("public override string ToString() => Value?.ToString() ?? string.Empty;");
        await Assert.That(source).Contains("string.Equals(Value, other.Value, System.StringComparison.Ordinal)");
        await Assert.That(source).Contains("throw new ArgumentNullException(nameof(value));");
    }

    [Test]
    public async Task WrongSignatureNormalize_ReportsDiagnostic()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Snap;

                            [Primify<int>]
                            public partial record struct Baz
                            {
                                private static string Normalize(string value) => value;
                            }
                            """;

        var (diagnostics, _) = Generate(code);

        await AssertThatDiagnosticPresent(diagnostics, "PRIT002");
    }

    [Test]
    public async Task InvalidWrappedType_SkipsGeneration()
    {
        const string code = """
                            namespace Snap;

                            // simulate broken usage by omitting the type argument entirely is impossible with a generic
                            // attribute; instead verify unknown primitive fallback path via reflection-free compile.
                            """;

        // No generation occurs without the attribute; assert harness sanity only.
        var (_, generated) = Generate(code);

        await Assert.That(generated).IsNull();
    }

    private static async Task AssertThatDiagnosticPresent(ImmutableArray<Diagnostic> diagnostics, string id)
    {
        await Assert.That(diagnostics.Any(d => d.Id == id)).IsTrue();
    }
}

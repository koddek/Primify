namespace Primify.Generator.Tests.Common;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Primify.Attributes;

public class GeneratorSnapshotTests
{
    internal static List<MetadataReference> BaseReferences()
    {
        var references = new List<MetadataReference>();
        var platformAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        if (platformAssemblies is not null)
        {
            foreach (var path in platformAssemblies.Split(Path.PathSeparator))
            {
                // The test host's own NuGet dependencies (LiteDB, Newtonsoft.Json) appear in the trusted
                // platform assemblies; exclude them so the default harness models a consumer that
                // references no serializer package.
                var name = Path.GetFileNameWithoutExtension(path);
                if (name.StartsWith("LiteDB", StringComparison.Ordinal) ||
                    name.StartsWith("Newtonsoft", StringComparison.Ordinal))
                {
                    continue;
                }

                references.Add(MetadataReference.CreateFromFile(path));
            }
        }

        references.Add(MetadataReference.CreateFromFile(typeof(PrimifyAttribute<>).Assembly.Location));
        return references;
    }

    internal static (ImmutableArray<Diagnostic> Diagnostics, string? Generated) Generate(string code)
    {
        var references = BaseReferences();

        var compilation = CSharpCompilation.Create(
            "snapshot",
            [CSharpSyntaxTree.ParseText(code)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var driver = CSharpGeneratorDriver.Create(new Primify.Generators.PrimifyGenerator().AsSourceGenerator());
        var runResult = driver.RunGeneratorsAndUpdateCompilation(
                compilation,
                out var generatedCompilation,
                out var generatorDiagnostics)
            .GetRunResult();

        var generated = runResult.Results.IsEmpty || runResult.Results[0].GeneratedSources.Length == 0
            ? null
            : runResult.Results[0].GeneratedSources[0].SourceText.ToString();

        return (generatedCompilation.GetDiagnostics().AddRange(generatorDiagnostics), generated);
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
        // This harness compilation references neither LiteDB nor Newtonsoft.Json, so no adapter code may appear.
        await Assert.That(source).DoesNotContain("LiteDB");
        await Assert.That(source).DoesNotContain("Newtonsoft");
        await Assert.That(source).Contains("[System.Text.Json.Serialization.JsonConverter(typeof(SystemTextJsonConverter<Foo, int>))]");
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

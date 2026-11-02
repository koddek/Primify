namespace Primify.Generator.Tests.Common;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Primify.Attributes;

/// <summary>
/// Compile-backed tests for the conditional serializer adapters: the generator must emit LiteDB and
/// Newtonsoft.Json wiring only when the consumer compilation references those packages.
/// </summary>
public class SerializerAdapterTests
{
    private const string WrapperCode = """
                                        using Primify.Attributes;

                                        namespace Snap;

                                        [Primify<int>]
                                        public readonly partial record struct Foo;
                                        """;

    private const string UnsupportedWrapperCode = """
                                                   using Primify.Attributes;

                                                   namespace Snap;

                                                   public sealed record Custom(string Text);

                                                   [Primify<Custom>]
                                                   public partial record struct CustomWrapper;
                                                   """;

    private static readonly MetadataReference LiteDbReference =
        MetadataReference.CreateFromFile(typeof(LiteDB.BsonMapper).Assembly.Location);

    private static readonly MetadataReference NewtonsoftReference =
        MetadataReference.CreateFromFile(typeof(Newtonsoft.Json.JsonConvert).Assembly.Location);

    private static readonly MetadataReference NewtonsoftBsonReference =
        MetadataReference.CreateFromFile(typeof(Newtonsoft.Json.Bson.BsonDataWriter).Assembly.Location);

    private static (ImmutableArray<Diagnostic> Diagnostics, ImmutableArray<GeneratedSourceResult> Sources) Generate(
        string code,
        params MetadataReference[] extraReferences)
    {
        var references = GeneratorSnapshotTests.BaseReferences();
        references.AddRange(extraReferences);

        var compilation = CSharpCompilation.Create(
            "adapters",
            [CSharpSyntaxTree.ParseText(code)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var driver = CSharpGeneratorDriver.Create(new Primify.Generators.PrimifyGenerator().AsSourceGenerator());
        var runResult = driver.RunGeneratorsAndUpdateCompilation(
                compilation,
                out var generatedCompilation,
                out var generatorDiagnostics)
            .GetRunResult();

        return (generatedCompilation.GetDiagnostics().AddRange(generatorDiagnostics), runResult.Results[0].GeneratedSources);
    }

    private static string AllSources(ImmutableArray<GeneratedSourceResult> sources) =>
        string.Join("\n", sources.Select(static source => source.SourceText.ToString()));

    private static async Task AssertNoErrors(ImmutableArray<Diagnostic> diagnostics)
    {
        var errors = diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToList();
        await Assert.That(errors).IsEmpty();
    }

    [Test]
    public async Task NoSerializerPackages_EmitsNoAdapterCode()
    {
        var (diagnostics, sources) = Generate(WrapperCode);

        await AssertNoErrors(diagnostics);
        await Assert.That(AllSources(sources)).DoesNotContain("LiteDB");
        await Assert.That(AllSources(sources)).DoesNotContain("Newtonsoft");
        await Assert.That(AllSources(sources)).Contains("SystemTextJsonConverter<Foo, int>");
    }

    [Test]
    public async Task LiteDbReferenced_EmitsRegistrationWithInlineConversions()
    {
        var (diagnostics, sources) = Generate(WrapperCode, LiteDbReference);

        await AssertNoErrors(diagnostics);
        var all = AllSources(sources);
        await Assert.That(all).Contains("global::LiteDB.BsonMapper.Global");
        await Assert.That(all).Contains("mapper.RegisterType<global::Snap.Foo>(");
        await Assert.That(all).Contains("serialize: wrapper => new global::LiteDB.BsonValue(wrapper.Value)");
        await Assert.That(all).Contains("deserialize: bson => global::Snap.Foo.From(bson.AsInt32)");
    }

    [Test]
    public async Task LiteDbReferenced_UnsupportedWrapper_ReportsPRIT007_AndSkipsRegistration()
    {
        var (diagnostics, sources) = Generate(UnsupportedWrapperCode, LiteDbReference);

        await AssertNoErrors(diagnostics);
        await Assert.That(diagnostics.Any(d => d.Id == "PRIT007" && d.Severity == DiagnosticSeverity.Warning)).IsTrue();
        await Assert.That(AllSources(sources)).DoesNotContain("RegisterType<global::Snap.CustomWrapper>");
    }

    [Test]
    public async Task NewtonsoftReferenced_EmitsConverterAndWrapperAttribute()
    {
        var (diagnostics, sources) = Generate(WrapperCode, NewtonsoftReference);

        await AssertNoErrors(diagnostics);
        var all = AllSources(sources);
        await Assert.That(all).Contains("[Newtonsoft.Json.JsonConverter(typeof(NewtonsoftJsonConverter<Foo, int>))]");
        await Assert.That(all).Contains("class NewtonsoftJsonConverter<TWrapper, TValue>");
        // Newtonsoft.Json.Bson is not referenced, so the BSON branches must not be emitted.
        await Assert.That(all).DoesNotContain("BsonDataWriter");
    }

    [Test]
    public async Task NewtonsoftBsonReferenced_EmitsBsonBranches()
    {
        var (diagnostics, sources) = Generate(WrapperCode, NewtonsoftReference, NewtonsoftBsonReference);

        await AssertNoErrors(diagnostics);
        await Assert.That(AllSources(sources)).Contains("BsonDataWriter");
    }
}

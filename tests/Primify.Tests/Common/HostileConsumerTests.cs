namespace Primify.Generator.Tests.Common;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

/// <summary>
/// Pins the behaviour the generator owes consumers that declare their own types in the same namespaces
/// Primify uses. Without global::-qualified emission this compilation fails with CS0104 (ambiguous
/// reference), because the consumer's <c>Primify.Converters.SystemTextJsonConverter&lt;,&gt;</c> would
/// compete with the real converter for the same name and arity.
/// </summary>
public class HostileConsumerTests
{
    [Test]
    public async Task ConsumerTypeNamedLikeAdapter_StillResolvesLibraryConverter()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Primify.Converters
                            {
                                public sealed class SystemTextJsonConverter<TWrapper, TValue>
                                {
                                }
                            }

                            namespace Snap
                            {
                                [Primify<int>]
                                public partial record struct HostileWrapper;
                            }
                            """;

        var (diagnostics, generated) = GeneratorSnapshotTests.Generate(code);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
        await Assert.That(errors).IsEmpty();
        await Assert.That(generated).Contains("global::Primify.Converters.SystemTextJsonConverter<HostileWrapper, int>");
    }
}

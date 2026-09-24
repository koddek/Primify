namespace Primify.Generator.Tests.Common;

public class GeneratedContractTests
{
    [Test]
    public async Task RefNormalizer_ReportsDiagnostic()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Snap;

                            [Primify.Attributes.Primify<int>]
                            public partial record struct RefNormalizer
                            {
                                private static int Normalize(ref int value) => value;
                            }
                            """;

        await AssertGeneratorRejectsAsync(code, "PRIT002");
    }

    [Test]
    public async Task GenericValidator_ReportsDiagnostic()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Snap;

                            [Primify.Attributes.Primify<int>]
                            public partial record struct GenericValidator
                            {
                                private static void Validate<T>(int value) { }
                            }
                            """;

        await AssertGeneratorRejectsAsync(code, "PRIT003");
    }

    [Test]
    public async Task ReservedValue_ReportsDiagnostic()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Snap;

                            [Primify.Attributes.Primify<int>]
                            public partial record struct ReservedValue
                            {
                                public int Value => 1;
                            }
                            """;

        await AssertGeneratorRejectsAsync(code, "PRIT006");
    }

    [Test]
    public async Task InvalidOverload_ReportsDiagnostic()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Snap;

                            [Primify.Attributes.Primify<int>]
                            public partial record struct InvalidOverload
                            {
                                private static int Normalize(int value) => value;
                                private static int Normalize(ref int value) => value;
                            }
                            """;

        await AssertGeneratorRejectsAsync(code, "PRIT002");
    }

    [Test]
    public async Task NormalizeProperty_ReportsDiagnostic()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Snap;

                            [Primify.Attributes.Primify<int>]
                            public partial record struct NormalizeProperty
                            {
                                private static int Normalize => 1;
                            }
                            """;

        await AssertGeneratorRejectsAsync(code, "PRIT002");
    }

    [Test]
    public async Task ValidateField_ReportsDiagnostic()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Snap;

                            [Primify.Attributes.Primify<int>]
                            public partial record struct ValidateField
                            {
                                private static int Validate => 1;
                            }
                            """;

        await AssertGeneratorRejectsAsync(code, "PRIT003");
    }

    [Test]
    public async Task NormalizeNull_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            _ = Types.NullReturningNormalizer.From("value");
        });
    }

    [Test]
    public async Task ClassTryFrom_ReturnsNullOnFailure()
    {
        var succeeded = Types.StringClassWithNormalizeAndValidate.TryFrom(" abcd ", out var result);

        await Assert.That(succeeded).IsFalse();
        await Assert.That(result).IsNull();
    }

    private static async Task AssertGeneratorRejectsAsync(string code, string diagnosticId)
    {
        var (diagnostics, generated) = GeneratorSnapshotTests.Generate(code);

        await Assert.That(diagnostics.Any(diagnostic => diagnostic.Id == diagnosticId)).IsTrue();
        await Assert.That(generated).IsNull();
    }
}

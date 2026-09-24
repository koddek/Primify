namespace Primify.Generator.Tests.Common;

public class GeneratorDeclarationTests
{
    [Test]
    public async Task GenericWrapper_IsRejected()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Snap;

                            [Primify<int>]
                            public partial record struct GenericWrapper<T>;
                            """;

        await AssertRejectedAsync(code, "generic wrappers");
    }

    [Test]
    public async Task GenericContainer_IsRejected()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Snap;

                            public partial class GenericContainer<T>
                            {
                                [Primify<int>]
                                public partial record struct Inner;
                            }
                            """;

        await AssertRejectedAsync(code, "generic wrappers");
    }

    [Test]
    public async Task NonPartialWrapper_IsRejected()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Snap;

                            [Primify<int>]
                            public record struct NonPartialWrapper;
                            """;

        await AssertRejectedAsync(code, "must be partial");
    }

    internal static async Task AssertRejectedAsync(string code, string expectedReason)
    {
        var (diagnostics, generated) = GeneratorSnapshotTests.Generate(code);

        var rejected = diagnostics.Any(diagnostic =>
            diagnostic.Id == "PRIT005" &&
            diagnostic.GetMessage().Contains(expectedReason, StringComparison.Ordinal));

        await Assert.That(rejected).IsTrue();
        await Assert.That(generated).IsNull();
    }
}

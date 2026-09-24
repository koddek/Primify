namespace Primify.Generator.Tests.Common;

public class GeneratorUnsupportedDeclarationTests
{
    [Test]
    public async Task StaticWrapper_IsRejected()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Snap;

                            [Primify.Attributes.Primify<int>]
                            public static partial class StaticWrapper;
                            """;

        await GeneratorDeclarationTests.AssertRejectedAsync(code, "static types");
    }

    [Test]
    public async Task AbstractWrapper_IsRejected()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Snap;

                            [Primify.Attributes.Primify<int>]
                            public abstract partial class AbstractWrapper;
                            """;

        await GeneratorDeclarationTests.AssertRejectedAsync(code, "abstract types");
    }

    [Test]
    public async Task FileLocalWrapper_IsRejected()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Snap;

                            [Primify.Attributes.Primify<int>]
                            file partial record struct FileWrapper;
                            """;

        await GeneratorDeclarationTests.AssertRejectedAsync(code, "file-local types");
    }

    [Test]
    public async Task RefStruct_IsRejected()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Snap;

                            [Primify.Attributes.Primify<int>]
                            public ref partial struct RefWrapper;
                            """;

        await GeneratorDeclarationTests.AssertRejectedAsync(code, "ref-like types");
    }

    [Test]
    public async Task PrivateNestedWrapper_IsRejected()
    {
        const string code = """
                            using Primify.Attributes;

                            namespace Snap;

                            public partial class Outer
                            {
                                [Primify.Attributes.Primify<int>]
                                private partial struct Inner;
                            }
                            """;

        await GeneratorDeclarationTests.AssertRejectedAsync(code, "must be accessible");
    }
}

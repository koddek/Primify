using Primify.Attributes;

namespace Primify.Generator.Tests.Types;

public sealed record UnsupportedLiteDbValue(string Text);

// LiteDB is referenced by this test project, so the generator reports PRIT007 for this bridge-less wrapper
// (suppressed project-wide in the .csproj; behavior is covered by SerializerAdapterTests).
[Primify<UnsupportedLiteDbValue>]
public partial record struct UnsupportedLiteDbWrapper;

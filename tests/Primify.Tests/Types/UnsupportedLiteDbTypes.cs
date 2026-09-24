using Primify.Attributes;

namespace Primify.Generator.Tests.Types;

public sealed record UnsupportedLiteDbValue(string Text);

[Primify<UnsupportedLiteDbValue>]
public partial record struct UnsupportedLiteDbWrapper;

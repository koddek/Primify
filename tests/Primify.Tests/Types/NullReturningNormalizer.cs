using Primify.Attributes;

namespace Primify.Generator.Tests.Types;

[Primify<string>]
public partial record struct NullReturningNormalizer
{
    private static string Normalize(string value) => null!;
}

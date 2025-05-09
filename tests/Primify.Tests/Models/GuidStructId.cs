using System;
using Primify.Attributes;

namespace Primify.Tests.Models;

[Primify<Guid>]
public readonly partial record struct GuidStructId
{
    // Example Predefined Value
    [PredefinedValue("22222222-2222-2222-2222-222222222222")]
    public static partial GuidStructId KnownStatic { get; }

    // User-defined Normalize implementation.
    private static partial Guid Normalize(Guid value) // Added partial back
    {
        return value;
    }

    // User-defined Validate implementation.
    // The generator provides 'static partial void Validate(Guid value);' definition.
    static partial void Validate(Guid value)
    {
        if (value == Guid.Parse("00000000-0000-0000-0000-000000000001")) // Arbitrary invalid Guid
        {
            throw new ArgumentException("GuidStructId cannot be '00000000-0000-0000-0000-000000000001'.", nameof(value));
        }
    }
}

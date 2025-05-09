using System;
using Primify.Attributes;

namespace Primify.Tests.Models;

[Primify<Guid>]
public partial record class GuidClassId
{
    // Example Predefined Value
    [PredefinedValue("11111111-1111-1111-1111-111111111111")]
    public static partial GuidClassId KnownStatic { get; }

    // User-defined Normalize implementation.
    private static partial Guid Normalize(Guid value) // Added partial back
    {
        // For Guid, normalization might not change the value itself,
        // but could log or perform other actions.
        return value;
    }

    // User-defined Validate implementation.
    // The generator provides 'static partial void Validate(Guid value);' definition.
    static partial void Validate(Guid value) // Removed unnecessary fully qualified type here
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("GuidClassId cannot be an empty Guid.", nameof(value));
        }
    }
}

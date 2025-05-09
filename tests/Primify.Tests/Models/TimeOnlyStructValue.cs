using System;
using Primify.Attributes;

namespace Primify.Tests.Models;

[Primify<TimeOnly>]
public readonly partial record struct TimeOnlyStructValue
{
    // Example Normalize: Round to the nearest second (downwards)
    private static partial TimeOnly Normalize(TimeOnly value) // Added partial back
    {
        return new TimeOnly(value.Hour, value.Minute, value.Second, 0); // Ignores milliseconds
    }

    // Example Validate: Milliseconds must be zero
    static partial void Validate(TimeOnly value) // Removed incorrect comment
    {
        if (value.Millisecond != 0)
        {
            throw new ArgumentException("TimeOnlyStructValue milliseconds must be zero.", nameof(value));
        }
    }
}

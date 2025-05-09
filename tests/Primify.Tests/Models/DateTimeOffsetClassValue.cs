using System;
using Primify.Attributes;

namespace Primify.Tests.Models;

[Primify<DateTimeOffset>]
public partial record class DateTimeOffsetClassValue
{
    // Example Normalize: Convert to UTC
    private static partial DateTimeOffset Normalize(DateTimeOffset value) // Added partial back
    {
        return value.ToUniversalTime();
    }

    // Example Validate: Must not be in the past (compared to UTC now)
    static partial void Validate(DateTimeOffset value) // Removed incorrect comment
    {
        // Value is already normalized to UTC by the Normalize method
        if (value < DateTimeOffset.UtcNow.Date) // Compare against the beginning of today UTC
        {
            throw new ArgumentOutOfRangeException(nameof(value), "DateTimeOffsetClassValue must not be in the past (date component).");
        }
    }
}

using System;
using Primify.Attributes;

namespace Primify.Tests.Models;

[Primify<DateTimeOffset>]
public readonly partial record struct DateTimeOffsetStructValue
{
    // Example Normalize: Ensure offset is +00:00 (UTC) but keep original DateTime part
    private static partial DateTimeOffset Normalize(DateTimeOffset value) // Added partial back
    {
        return new DateTimeOffset(value.DateTime, TimeSpan.Zero);
    }

    // Example Validate: Year must be current year or future
    static partial void Validate(DateTimeOffset value) // Removed incorrect comment
    {
        // Value is normalized to have a UTC offset by Normalize method
        if (value.Year < DateTimeOffset.UtcNow.Year)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"DateTimeOffsetStructValue year must be {DateTimeOffset.UtcNow.Year} or later.");
        }
    }
}

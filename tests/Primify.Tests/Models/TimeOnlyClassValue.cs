using System;
using Primify.Attributes;

namespace Primify.Tests.Models;

[Primify<TimeOnly>]
public partial record class TimeOnlyClassValue
{
    // Example Normalize: Round to the nearest minute (downwards)
    private static partial TimeOnly Normalize(TimeOnly value) // Added partial back
    {
        return new TimeOnly(value.Hour, value.Minute, 0, 0); // Ignores seconds and milliseconds
    }

    // Example Validate: Must be between 9 AM and 5 PM
    static partial void Validate(TimeOnly value) // Removed fully qualified type
    {
        var nineAm = new TimeOnly(9, 0);
        var fivePm = new TimeOnly(17, 0);
        if (value < nineAm || value > fivePm)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "TimeOnlyClassValue must be between 9 AM and 5 PM (inclusive).");
        }
    }
}

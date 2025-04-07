using System;
using Primify.Attributes;

namespace Primify.Tests.Models;

[Primify<int>]
public partial record class Number
{
    [PredefinedValue(0)]
    public static partial Number Empty { get; }

    [PredefinedValue(-1)]
    public static partial Number Undefined { get; }

    // Custom validation logic
    static partial void Validate(int value)
    {
        if (value < 1)
            throw new ArgumentException("Invalid email number");
    }
}

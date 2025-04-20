using System;
using Primify.Attributes;

namespace Primify.Tests.Models;

[Primify<string>]
public readonly partial record struct ItemName
{
    [PredefinedValue("")]
    public static partial ItemName Empty { get; }

    [PredefinedValue("guest")]
    public static partial ItemName Undefined { get; }

    // Custom validation logic
    static partial void Validate(string value)
    {
        if (value.Contains("@") || string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Invalid username address");
    }

    private static partial string Normalize(string value)
    {
        return value.Trim();
    }
}

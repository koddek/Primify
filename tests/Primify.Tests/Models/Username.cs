using System;
using Primify.Attributes;

namespace Primify.Tests.Models;

[Primify<string>]
public partial record class Username
{
    [PredefinedValue("")]
    public static partial Username Empty { get; }

    [PredefinedValue("guest")]
    public static partial Username Undefined { get; }

    // Custom validation logic
    static partial void Validate(string value)
    {
        if (value.Contains("@") || string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Invalid username address");
    }

    private static partial string Normalize(string value)
    {
        return value.ToLower();
    }
}

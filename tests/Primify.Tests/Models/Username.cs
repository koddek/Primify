using System;
using Primify.Attributes;

namespace Primify.Tests.Models;

[Primify<string>]
public partial record class Username
{
    /*[PredefinedValue("")]
    static readonly Username _empty;

    [PredefinedValue("undefined@example.com")]
    static readonly Username _undefined;*/

    // Custom validation logic
    static partial void Validate(string value)
    {
        if (!value.Contains("@") && !string.IsNullOrEmpty(value))
            throw new ArgumentException("Invalid email address");
    }

    private static partial string Normalize(string value)
    {
        return value.ToLower();
    }
}

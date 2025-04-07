using System;
using Primify.Attributes;

namespace Primify.Tests.Models;

[Primify<string>]
public partial record class EmailAddress
{
    [PredefinedValue("")]
    public static partial EmailAddress Empty { get; }

    [PredefinedValue("undefined@example.com")]
    public static partial EmailAddress Undefined { get; }

    // Custom validation logic
    static partial void Validate(string value)
    {
        if (!value.Contains("@") && !string.IsNullOrEmpty(value))
            throw new ArgumentException("Invalid email address");
    }
}

using System;
using Primify.Attributes;

namespace Primify.Tests.Models;

[Primify<string>]
public sealed partial record class ItemName
{
    private static partial string Normalize(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}

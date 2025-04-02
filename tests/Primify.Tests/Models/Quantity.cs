using System;
using Primify.Attributes;

namespace Primify.Tests.Models;

[Primify<int>]
public sealed partial record class Quantity
{
    static partial void Validate(int value)
    {
        if (value <= 0)
            throw new ArgumentException("Quantity must be positive.");
    }
}

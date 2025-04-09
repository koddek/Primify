using System;
using Primify.Attributes;

namespace Primify.Tests.Models;

[Primify<DateOnly>]
public partial record class DayId
{
    public static DayId Undefined {get;} = new(DateOnly.MinValue);
}
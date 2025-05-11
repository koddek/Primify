using System;
using Primify.Attributes;

namespace Primify.Tests.Models;

[Primify<DateOnly>]
public partial record class DayId
{
    [PredefinedValue("0001-01-01")]
    public static partial DayId Empty {get;}
    public static DayId Undefined {get;} = new(DateOnly.MinValue);
}

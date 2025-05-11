using System;
using Primify.Attributes;

namespace Primify.Tests.Models;

[Primify<DateOnly>]
public partial record class DayId
{
    [PredefinedValue("0001-01-01")] public static partial DayId Empty { get; }
    public static DayId Undefined { get; } = new(DateOnly.MinValue);
}

[Primify<TimeOnly>]
public partial record class TimeId
{
    [PredefinedValue("00:00:00")] public static partial TimeId Empty { get; }
}

[Primify<DateTime>]
public partial record class DateTimeId
{
    [PredefinedValue("0001-01-01T00:00:00")]
    public static partial DateTimeId Empty { get; }
}

[Primify<DateTimeOffset>]
public partial record class DateTimeOffsetId
{
    [PredefinedValue("0001-01-01T00:00:00+00:00")]
    public static partial DateTimeOffsetId Empty { get; }
}

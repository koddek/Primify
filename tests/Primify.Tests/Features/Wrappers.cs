using System;
using Primify.Attributes;

namespace Primify.Tests.Features;

// Wrapper Definitions

[Primify<int>]
public readonly partial record struct IntStructWrapper
{
    [PredefinedValue(0)] public static partial IntStructWrapper Zero { get; }
    [PredefinedValue(1)] public static partial IntStructWrapper One { get; }

    private static partial int Normalize(int value) => value != -1 ? Math.Abs(value) : value;

    static partial void Validate(int value)
    {
        if (value == -1)
            throw new ArgumentException("‘-1’ is not allowed for this wrapper");
    }
}

[Primify<int>]
public partial record class IntClassWrapper
{
    [PredefinedValue(0)] public static partial IntClassWrapper Zero { get; }
    [PredefinedValue(1)] public static partial IntClassWrapper One { get; }

    static partial void Validate(int value)
    {
        if (value < 0) throw new ArgumentException("Value cannot be negative");
    }
}

[Primify<double>]
public readonly partial record struct DoubleStructWrapper
{
    private static partial double Normalize(double value) => Math.Abs(value);
}

[Primify<double>]
public partial record class DoubleClassWrapper
{
    private static partial double Normalize(double value) => Math.Abs(value);
}

[Primify<string>]
public readonly partial record struct StringStructWrapper
{
    private static partial string Normalize(string value) => value.Trim();

    static partial void Validate(string value)
    {
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("Value cannot be null or empty");
    }
}

[Primify<string>]
public partial record class StringClassWrapper
{
    private static partial string Normalize(string value) => value.Trim();

    static partial void Validate(string value)
    {
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("Value cannot be null or empty");
    }
}

[Primify<bool>]
public readonly partial record struct BoolStructWrapper
{
    [PredefinedValue(true)] public static partial BoolStructWrapper True { get; }
    [PredefinedValue(false)] public static partial BoolStructWrapper False { get; }
}

[Primify<bool>]
public partial record class BoolClassWrapper
{
    [PredefinedValue(true)] public static partial BoolClassWrapper True { get; }
    [PredefinedValue(false)] public static partial BoolClassWrapper False { get; }
}

[Primify<DateTime>]
public readonly partial record struct DateTimeStructWrapper
{
    private static partial DateTime Normalize(DateTime value) => value.ToUniversalTime();
}

[Primify<DateTime>]
public partial record class DateTimeClassWrapper
{
    private static partial DateTime Normalize(DateTime value) => value.ToUniversalTime();
}

[Primify<Guid>]
public readonly partial record struct GuidStructWrapper
{
    static partial void Validate(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Value cannot be empty");
    }
}

[Primify<Guid>]
public partial record class GuidClassWrapper
{
    static partial void Validate(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Value cannot be empty");
    }
}

[Primify<DateOnly>]
public readonly partial record struct DateOnlyStructWrapper
{
    // Validate that the date is not in the past
    static partial void Validate(DateOnly value)
    {
        if (value < DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("DateOnlyStructWrapper cannot be in the past");
    }

    // No normalization needed for DateOnly
    private static partial DateOnly Normalize(DateOnly value) => value;
}

[Primify<DateOnly>]
public partial record class DateOnlyClassWrapper
{
    // Validate that the date is not in the past
    static partial void Validate(DateOnly value)
    {
        if (value < DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("DateOnlyClassWrapper cannot be in the past");
    }

    // No normalization needed for DateOnly
    private static partial DateOnly Normalize(DateOnly value) => value;
}

[Primify<TimeOnly>]
public readonly partial record struct TimeOnlyStructWrapper
{
    // Validate that the time is between 9 AM and 5 PM (business hours)
    static partial void Validate(TimeOnly value)
    {
        var businessStart = new TimeOnly(9, 0);
        var businessEnd = new TimeOnly(17, 0);

        if (value < businessStart || value > businessEnd)
            throw new ArgumentException("TimeOnlyStructWrapper must be between 9 AM and 5 PM");
    }

    // Round to the nearest minute for normalization
    private static partial TimeOnly Normalize(TimeOnly value)
    {
        // Round to nearest minute by removing seconds and adding 30 seconds
        if (value.Second >= 30)
        {
            return new TimeOnly(value.Hour, value.Minute).AddMinutes(1);
        }

        return new TimeOnly(value.Hour, value.Minute);
    }
}

[Primify<TimeOnly>]
public partial record class TimeOnlyClassWrapper
{
    // Validate that the time is between 9 AM and 5 PM (business hours)
    static partial void Validate(TimeOnly value)
    {
        var businessStart = new TimeOnly(9, 0);
        var businessEnd = new TimeOnly(17, 0);

        if (value < businessStart || value > businessEnd)
            throw new ArgumentException("TimeOnlyClassWrapper must be between 9 AM and 5 PM");
    }

    // Round to the nearest minute for normalization
    private static partial TimeOnly Normalize(TimeOnly value)
    {
        // Round to nearest minute by removing seconds and adding 30 seconds
        if (value.Second >= 30)
        {
            return new TimeOnly(value.Hour, value.Minute).AddMinutes(1);
        }

        return new TimeOnly(value.Hour, value.Minute);
    }
}

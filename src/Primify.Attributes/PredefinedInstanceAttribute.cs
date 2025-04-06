using System;

namespace Primify.Attributes;

/// <summary>
/// Marks a static property as a predefined instance of a value object with the specified value.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class PredefinedValueAttribute : Attribute
{
    /// <summary>
    /// The primitive value to use for this predefined instance.
    /// </summary>
    public object Value { get; }

    /// <summary>
    /// Creates a new instance of the PredefinedValueAttribute with the specified value.
    /// </summary>
    /// <param name="value">The primitive value for the predefined instance.</param>
    public PredefinedValueAttribute(object value)
    {
        Value = value;
    }
}

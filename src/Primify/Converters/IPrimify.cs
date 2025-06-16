namespace Primify.Converters;

/// <summary>
/// Defines the contract for a Primify type, enabling reflection-free operations.
/// Requires C# 11 / .NET 7 or higher.
/// </summary>
/// <typeparam name="TSelf">The concrete wrapper type that implements this interface.</typeparam>
/// <typeparam name="TValue">The underlying primitive value type.</typeparam>
public interface IPrimify<out TSelf, TValue> where TSelf : IPrimify<TSelf, TValue>
{
    /// <summary>
    /// Gets the underlying primitive value of the wrapper.
    /// </summary>
    TValue Value { get; }

    /// <summary>
    /// Creates an instance of the wrapper type from a primitive value.
    /// This static method is the key feature enabling reflection-free deserialization.
    /// </summary>
    static abstract TSelf From(TValue value);
}

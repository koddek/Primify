namespace Primify;

using System;

/// <summary>
/// The contract every Primify-generated wrapper implements. It is what makes serializer plumbing
/// reflection-free: a converter reads <see cref="Value"/> directly and rebuilds the instance through
/// <see cref="From(TValue)"/>, so normalization and validation run during deserialization exactly as
/// they do at construction sites.
/// </summary>
/// <remarks>
/// <para>
/// Generated wrappers also expose <c>TryFrom</c>, the conversion operators, <c>ToString</c>, and the
/// equality members. Those are generated per wrapper and deliberately not part of this interface.
/// </para>
/// <para>
/// Generic code constrains on this interface with a single <c>using Primify;</c>. The attribute lives in
/// <see cref="Attributes.PrimifyAttribute{TPrimitive}"/> and the serializer adapters in
/// <c>Primify.Converters</c>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Generic helper that works over any Primify wrapper without reflection:
/// static TSelf Remap&lt;TSelf, TValue&gt;(TSelf wrapper, Func&lt;TValue, TValue&gt; transform)
///     where TSelf : IPrimify&lt;TSelf, TValue&gt;
///     =&gt; TSelf.From(transform(wrapper.Value));
/// </code>
/// </example>
/// <typeparam name="TSelf">The concrete wrapper type that implements this interface.</typeparam>
/// <typeparam name="TValue">The underlying primitive value type.</typeparam>
public interface IPrimify<out TSelf, TValue> where TSelf : IPrimify<TSelf, TValue>
{
    /// <summary>
    /// Gets the underlying primitive value of the wrapper, after normalization and validation.
    /// </summary>
    TValue Value { get; }

    /// <summary>
    /// Creates a wrapper from a primitive value, applying the declaring type's <c>Normalize</c> and
    /// <c>Validate</c> hooks. Rejects null reference values and anything validation throws for, so a
    /// successful return is always a valid instance.
    /// </summary>
    /// <param name="value">The primitive value to wrap.</param>
    /// <returns>The validated wrapper instance.</returns>
    /// <exception cref="ArgumentException">The value was rejected by the wrapper's hooks.</exception>
    static abstract TSelf From(TValue value);
}

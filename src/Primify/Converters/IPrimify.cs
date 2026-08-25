namespace Primify.Converters;

/// <summary>
/// Contract implemented by every Primify-generated wrapper type. Enables reflection-free
/// serializer plumbing: converters read <see cref="Value"/> directly and rebuild instances
/// through <see cref="From(TValue)"/>, so normalization and validation run during
/// deserialization exactly as they do at construction sites.
/// </summary>
/// <remarks>
/// Generated wrappers also expose <c>TryFrom</c>, explicit/implicit conversion operators,
/// <c>ToString</c>, and equality members; those are generated per type and intentionally not
/// part of this interface.
/// </remarks>
/// <example>
/// <code>
/// // Generic helper working over any Primify wrapper without reflection:
/// static TSelf Normalize&lt;TSelf, TValue&gt;(TSelf wrapper, Func&lt;TValue, TValue&gt; transform)
///     where TSelf : IPrimify&lt;TSelf, TValue&gt;
///     =&gt; TSelf.From(transform(wrapper.Value));
/// </code>
/// </example>
/// <typeparam name="TSelf">The concrete wrapper type that implements this interface.</typeparam>
/// <typeparam name="TValue">The underlying primitive value type.</typeparam>
public interface IPrimify<out TSelf, TValue> where TSelf : IPrimify<TSelf, TValue>
{
    /// <summary>
    /// Gets the underlying primitive value of the wrapper.
    /// </summary>
    TValue Value { get; }

    /// <summary>
    /// Creates an instance of the wrapper from a primitive value, applying the declaring type's
    /// <c>Normalize</c> and <c>Validate</c> hooks. Throws if validation rejects the value.
    /// This static abstract member is what enables reflection-free deserialization.
    /// </summary>
    static abstract TSelf From(TValue value);
}

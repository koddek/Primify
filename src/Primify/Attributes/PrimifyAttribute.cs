namespace Primify.Attributes;

using System;

/// <summary>
/// Marks a partial type for Primify to complete into a strongly-typed wrapper around
/// <typeparamref name="TPrimitive"/>, so a value like an email address or an order id cannot be passed
/// around as a bare primitive.
/// </summary>
/// <remarks>
/// <para>
/// The generator adds a read-only <c>Value</c> property, a private constructor, <c>From</c> and
/// <c>TryFrom</c> factories, the conversion operators, equality members, the serializer attributes, and a
/// LiteDB mapping registration.
/// </para>
/// <para>
/// The hand-written part may optionally declare two private static hooks. They run in this order for
/// non-null values entering through <c>From</c>, <c>TryFrom</c>, the explicit conversion, or
/// deserialization:
/// </para>
/// <list type="number">
/// <item><description><c>private static TPrimitive Normalize(TPrimitive value)</c> — cleanup and transformation.</description></item>
/// <item><description><c>private static void Validate(TPrimitive value)</c> — invariant checks. Throw an
/// <see cref="ArgumentException"/>-derived exception to reject the value.</description></item>
/// </list>
/// <para>
/// The declaration must be non-generic, non-static, non-abstract, non-file-local, not ref-like, partial,
/// and reachable from generated namespace-level code. Each containing type must be partial too.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [Primify&lt;string&gt;]
/// public sealed partial record class EmailAddress
/// {
///     private static string Normalize(string value) =&gt; value.Trim().ToLowerInvariant();
///
///     private static void Validate(string value)
///     {
///         if (!value.Contains('@'))
///             throw new ArgumentException("Not a valid email address.", nameof(value));
///     }
/// }
/// </code>
/// </example>
/// <typeparam name="TPrimitive">The primitive or built-in type to wrap.</typeparam>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PrimifyAttribute<TPrimitive> : Attribute
{
}

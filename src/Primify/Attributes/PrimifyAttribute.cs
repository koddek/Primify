namespace Primify.Attributes
{
    /// <summary>
    /// Marks a partial type to be completed by the Primify source generator as a strongly-typed
    /// wrapper around <typeparamref name="TPrimitive"/>, eliminating primitive obsession.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The generator adds a read-only <c>Value</c> property typed
    /// <typeparamref name="TPrimitive"/>, a private constructor, factory methods,
    /// conversions, equality members, serializer attributes, and a LiteDB mapping registration.
    /// </para>
    /// <para>
    /// The declaring (user-written) part may optionally define the following private static hooks,
    /// which the generator calls in this order whenever a value enters through
    /// <c>From</c>/<c>TryFrom</c>/explicit conversion/deserialization:
    /// </para>
    /// <list type="number">
    /// <item><c>private static TPrimitive Normalize(TPrimitive value)</c> — cleanup/transformation.</item>
    /// <item><c>private static void Validate(TPrimitive value)</c> — invariant checks; throw an
    /// <see cref="ArgumentException"/>-derived exception to reject the value.</item>
    /// </list>
    /// <para>
    /// Hooks with any other visibility or signature cause compile-time diagnostics PRIT002/PRIT003.
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
    [System.AttributeUsage(System.AttributeTargets.Struct | System.AttributeTargets.Class, AllowMultiple = false,
        Inherited = false)]
    public sealed class PrimifyAttribute<TPrimitive> : Attribute
    {
    }
}

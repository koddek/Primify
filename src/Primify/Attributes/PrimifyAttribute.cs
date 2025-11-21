namespace Primify.Attributes
{
    /// <summary>
    /// Marks a type to be generated as a value wrapper type for the specified primitive type.
    /// </summary>
    /// <typeparam name="TPrimitive">The primitive type to Primify.</typeparam>
    [System.AttributeUsage(System.AttributeTargets.Struct | System.AttributeTargets.Class, AllowMultiple = false,
        Inherited = false)]
    public sealed class PrimifyAttribute<TPrimitive> : Attribute
    {
    }
}

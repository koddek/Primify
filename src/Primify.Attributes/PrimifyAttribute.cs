namespace Primify.Attributes;

/// <summary>
/// Marks a type to be generated as a value wrapper type for the specified primitive type.
/// </summary>
/// <typeparam name="TPrimitive">The primitive type to Primify.</typeparam>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class PrimifyAttribute<TPrimitive> : Attribute;

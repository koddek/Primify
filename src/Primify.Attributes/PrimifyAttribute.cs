using System;

namespace Primify.Attributes;

/// <summary>
/// Marks a type to be generated as a value wrapper type for the specified primitive type.
/// </summary>
/// <typeparam name="T">The primitive type to wrap.</typeparam>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class PrimifyAttribute<T> : Attribute;

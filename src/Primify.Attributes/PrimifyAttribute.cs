using System;

namespace Primify.Attributes;

/// <summary>
/// Indicates that a record class or readonly record struct should be treated as a value wrapper
/// that can be automatically serialized to/from its underlying primitive type.
/// </summary>
/// <typeparam name="T">The primitive type that this wrapper encapsulates.</typeparam>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class PrimifyAttribute<T> : Attribute;

namespace System.Runtime.CompilerServices;

// netstandard2.0 does not ship the compiler-feature attributes that `closed` types require, so the
// analyzer carries its own internal copies. They are internal to this assembly and never visible
// to consumers; the closed-hierarchy checks are a compile-time feature.

using System;

/// <summary>Marks the type as the base of a closed hierarchy; only this assembly may derive from it.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
internal sealed class IsClosedTypeAttribute : Attribute;

/// <summary>Records that a member was emitted using a compiler feature smaller runtimes lack.</summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
internal sealed class CompilerFeatureRequiredAttribute : Attribute
{
    /// <summary>Initializes a new instance of the attribute with the feature name.</summary>
    public CompilerFeatureRequiredAttribute(string featureName) => FeatureName = featureName;

    /// <summary>Initializes a new instance of the attribute with the feature name and optionality flag.</summary>
    public CompilerFeatureRequiredAttribute(string featureName, bool isOptional)
        : this(featureName) => IsOptional = isOptional;

    /// <summary>Gets the name of the compiler feature.</summary>
    public string FeatureName { get; }

    /// <summary>Gets whether the feature can be ignored on older compilers.</summary>
    public bool IsOptional { get; }
}

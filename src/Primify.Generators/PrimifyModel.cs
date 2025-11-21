namespace Primify.Generators;

/// <summary>
/// Represents the data required to generate a Primify wrapper.
/// This is a record to ensure value-based equality for Roslyn caching.
/// </summary>
public record PrimifyModel(
    string Namespace,
    string ClassName,
    string Keyword, // class, struct, record struct
    string WrappedType, // int, string, Guid
    bool IsValueType,
    bool IsRecord,
    bool HasNormalize,
    bool HasValidate,
    Location Location // For reporting diagnostics
);

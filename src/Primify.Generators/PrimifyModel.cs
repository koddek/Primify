namespace Primify.Generators;

using Microsoft.CodeAnalysis.Text;

/// <summary>
/// How a generated wrapper compares its underlying value. Chosen from the wrapped symbol's
/// <see cref="Microsoft.CodeAnalysis.SpecialType"/> so the decision never depends on how the user
/// spelled or qualified the type.
/// </summary>
internal enum EqualityStrategy
{
    /// <summary>Compare with <c>string.Equals(a, b, StringComparison.Ordinal)</c>.</summary>
    OrdinalString,

    /// <summary>Compare with the type's own <c>==</c> operator, which is reflexive for that type.</summary>
    Operator,

    /// <summary>Compare with <c>EqualityComparer&lt;T&gt;.Default</c>. Required for types whose
    /// <c>==</c> is not reflexive, such as the floating-point types where <c>NaN != NaN</c>.</summary>
    EqualityComparer
}

/// <summary>
/// Represents the data required to generate a Primify wrapper.
/// This is a record to ensure value-based equality for Roslyn caching.
/// </summary>
internal record PrimifyModel(
    string Namespace,
    string ClassName,
    string Keyword, // class, struct, record class, record struct
    string WrappedType, // int, string, Guid, ...
    bool WrappedTypeIsReferenceType,
    bool IsValueType,
    bool IsRecord,
    EqualityStrategy EqualityStrategy,
    bool HasNormalize,
    bool HasValidate,
    bool InvalidNormalizeSignature,
    bool InvalidValidateSignature,
    string? UnsupportedDeclarationReason,
    string? ReservedMemberConflict,
    DiagnosticLocation Location,
    EquatableArray<ContainingTypeModel> ContainingTypes,
    bool LiteDbAvailable,
    LiteDbStorageKind LiteDbStorage,
    bool NewtonsoftAvailable,
    bool NewtonsoftBsonAvailable
)
{
    public bool IsGlobalNamespace => string.IsNullOrWhiteSpace(Namespace) || Namespace == "<global namespace>";

    public string TypeName => ContainingTypes.IsEmpty
        ? ClassName
        : $"{ContainingTypes.Join(static type => type.Name)}.{ClassName}";

    public string TypeReferenceName => ContainingTypes.IsEmpty
        ? ClassName
        : $"{ContainingTypes.Join(static type => type.TypeReferenceName)}.{ClassName}";

    public string FullyQualifiedTypeName => IsGlobalNamespace
        ? TypeReferenceName
        : $"global::{Namespace}.{TypeReferenceName}";

    public string HintName => IsGlobalNamespace
        ? $"{TypeName}.g.cs"
        : $"{Namespace}.{TypeName}.g.cs";

    public bool HasUnsupportedContainingType => ContainingTypes.Any(type => !type.IsPartial);

    /// <summary>True when the wrapper itself is generated, i.e. no blocking diagnostic was reported.</summary>
    public bool IsGeneratable =>
        WrappedType != "object" &&
        UnsupportedDeclarationReason is null &&
        ReservedMemberConflict is null &&
        !InvalidNormalizeSignature &&
        !InvalidValidateSignature &&
        !HasUnsupportedContainingType;
}

/// <summary>
/// An equatable snapshot of a diagnostic position. Holding <see cref="Location"/>
/// directly would defeat the value-based equality required for incremental caching.
/// </summary>
internal readonly record struct DiagnosticLocation(
    string FilePath,
    TextSpan SourceSpan,
    LinePositionSpan LineSpan)
{
    public static DiagnosticLocation From(Location location) => new(
        location.SourceTree?.FilePath ?? string.Empty,
        location.SourceSpan,
        location.GetLineSpan().Span);

    public Location ToLocation() => Location.Create(FilePath, SourceSpan, LineSpan);
}

internal record ContainingTypeModel(
    string Declaration,
    string Name,
    string TypeReferenceName,
    bool IsPartial,
    bool IsGeneric
);

internal readonly record struct EquatableArray<T>(T[] Items) : IEquatable<EquatableArray<T>>
    where T : IEquatable<T>
{
    public static EquatableArray<T> Empty { get; } = new([]);

    public bool IsEmpty => Items is null || Items.Length == 0;

    public bool Any(Func<T, bool> predicate) => Items is not null && Items.Any(predicate);

    public string Join(Func<T, string> selector) => Items is null
        ? string.Empty
        : string.Join(".", Items.Select(selector));

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)(Items ?? [])).GetEnumerator();

    public bool Equals(EquatableArray<T> other) => (Items ?? []).SequenceEqual(other.Items ?? []);

    public override int GetHashCode()
    {
        var hash = 17;
        foreach (var item in Items ?? [])
        {
            hash = (hash * 31) + item.GetHashCode();
        }

        return hash;
    }
}

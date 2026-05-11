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
    Location Location, // For reporting diagnostics
    EquatableArray<ContainingTypeModel> ContainingTypes
)
{
    public string TypeName => ContainingTypes.IsEmpty
        ? ClassName
        : $"{ContainingTypes.Join(static type => type.Name)}.{ClassName}";

    public string TypeReferenceName => ContainingTypes.IsEmpty
        ? ClassName
        : $"{ContainingTypes.Join(static type => type.TypeReferenceName)}.{ClassName}";

    public string FullyQualifiedTypeName => string.IsNullOrWhiteSpace(Namespace) || Namespace == "<global namespace>"
        ? TypeReferenceName
        : $"global::{Namespace}.{TypeReferenceName}";

    public string HintName => string.IsNullOrWhiteSpace(Namespace) || Namespace == "<global namespace>"
        ? $"{TypeName}.g.cs"
        : $"{Namespace}.{TypeName}.g.cs";

    public bool HasUnsupportedContainingType => ContainingTypes.Any(type => !type.IsPartial);
}

public record ContainingTypeModel(
    string Declaration,
    string Name,
    string TypeReferenceName,
    bool IsPartial
);

public readonly record struct EquatableArray<T>(T[] Items) : IEquatable<EquatableArray<T>>
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

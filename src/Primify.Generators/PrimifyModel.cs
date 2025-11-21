namespace Primify.Generators;

public sealed class PrimifyModel(
    string ns,
    string className,
    string keyword,
    string wrapperArgument,
    bool isValueType,
    bool isRecord,
    bool hasNormalize,
    bool hasValidate,
    Location location
) : IEquatable<PrimifyModel>
{
    public string Namespace { get; } = ns;
    public string ClassName { get; } = className;
    public string Keyword { get; } = keyword;
    public string WrapperArgument { get; } = wrapperArgument;
    public bool IsValueType { get; } = isValueType;
    public bool IsRecord { get; } = isRecord;
    public bool HasNormalize { get; } = hasNormalize;
    public bool HasValidate { get; } = hasValidate;
    public Location Location { get; } = location;

    // Add to constructor

    public bool Equals(PrimifyModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Namespace == other.Namespace &&
               ClassName == other.ClassName &&
               Keyword == other.Keyword &&
               WrapperArgument == other.WrapperArgument &&
               IsValueType == other.IsValueType &&
               IsRecord == other.IsRecord &&
               HasNormalize == other.HasNormalize &&
               HasValidate == other.HasValidate &&
               Location == other.Location;
    }

    public override bool Equals(object? obj) =>
        ReferenceEquals(this, obj) || obj is PrimifyModel other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (Namespace != null ? Namespace.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (ClassName != null ? ClassName.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Keyword != null ? Keyword.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (WrapperArgument != null ? WrapperArgument.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ IsValueType.GetHashCode();
            hashCode = (hashCode * 397) ^ IsRecord.GetHashCode();
            hashCode = (hashCode * 397) ^ HasNormalize.GetHashCode();
            hashCode = (hashCode * 397) ^ HasValidate.GetHashCode();
            hashCode = (hashCode * 397) ^ Location.GetHashCode();
            return hashCode;
        }
    }
}

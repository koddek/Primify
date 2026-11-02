namespace Primify.Generators;

using Microsoft.CodeAnalysis;

/// <summary>
/// The BSON shape a wrapped type converts to when the consumer references LiteDB. LiteDB stores only
/// Null, Int32, Int64, Double, Decimal, String, Document, Array, Binary, ObjectId, Guid, Boolean, and
/// millisecond-precision UTC DateTime, so wrappers of other .NET types bridge onto those shapes at the
/// point of serialization. <see cref="None"/> means no built-in bridge exists.
/// </summary>
internal enum LiteDbStorageKind
{
    /// <summary>No built-in bridge; the wrapper needs a manual <c>BsonMapper.RegisterType</c> call.</summary>
    None,

    /// <summary>Stored as a BSON String.</summary>
    String,

    /// <summary>Stored as a BSON Boolean.</summary>
    Boolean,

    /// <summary>Stored as a BSON Int32 (int directly; byte, sbyte, short, ushort, char widened with a checked cast on read).</summary>
    Int32,

    /// <summary>Stored as a BSON Int64.</summary>
    Int64,

    /// <summary>uint widened to BSON Int64, following LiteDB's own UInt32 to Int64 convention.</summary>
    UInt32,

    /// <summary>ulong stored as BSON Int64; values beyond long.MaxValue throw OverflowException.</summary>
    UInt64,

    /// <summary>Stored as a BSON Double.</summary>
    Double,

    /// <summary>float widened to BSON Double.</summary>
    Single,

    /// <summary>Half widened to BSON Double.</summary>
    Half,

    /// <summary>Stored as a BSON Decimal.</summary>
    Decimal,

    /// <summary>Stored as a BSON Guid.</summary>
    Guid,

    /// <summary>DateTime stored as a BSON DateTime normalized to UTC (LiteDB keeps millisecond precision).</summary>
    UtcDateTime,

    /// <summary>TimeSpan stored as BSON Int64 ticks.</summary>
    TimeSpanTicks,

    /// <summary>TimeOnly stored as BSON Int64 ticks.</summary>
    TimeOnlyTicks,

    /// <summary>DateOnly stored as a UTC-midnight BSON DateTime.</summary>
    DateOnlyUtcMidnight,

    /// <summary>DateTimeOffset stored as a { UtcTicks, OffsetTicks } document, exact to the tick.</summary>
    DateTimeOffsetDocument
}

/// <summary>
/// Maps a wrapped type symbol to the BSON shape the generated LiteDB registration uses for it.
/// </summary>
internal static class LiteDbStorage
{
    public static LiteDbStorageKind Classify(ITypeSymbol? wrappedType)
    {
        switch (wrappedType?.SpecialType)
        {
            case SpecialType.System_String:
                return LiteDbStorageKind.String;
            case SpecialType.System_Boolean:
                return LiteDbStorageKind.Boolean;
            case SpecialType.System_Int32:
            case SpecialType.System_Byte:
            case SpecialType.System_SByte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Char:
                return LiteDbStorageKind.Int32;
            case SpecialType.System_Int64:
                return LiteDbStorageKind.Int64;
            case SpecialType.System_UInt32:
                return LiteDbStorageKind.UInt32;
            case SpecialType.System_UInt64:
                return LiteDbStorageKind.UInt64;
            case SpecialType.System_Double:
                return LiteDbStorageKind.Double;
            case SpecialType.System_Single:
                return LiteDbStorageKind.Single;
            case SpecialType.System_Decimal:
                return LiteDbStorageKind.Decimal;
            case SpecialType.System_DateTime:
                return LiteDbStorageKind.UtcDateTime;
        }

        return wrappedType switch
        {
            _ when IsSystemType(wrappedType, "Guid") => LiteDbStorageKind.Guid,
            _ when IsSystemType(wrappedType, "Half") => LiteDbStorageKind.Half,
            _ when IsSystemType(wrappedType, "TimeSpan") => LiteDbStorageKind.TimeSpanTicks,
            _ when IsSystemType(wrappedType, "TimeOnly") => LiteDbStorageKind.TimeOnlyTicks,
            _ when IsSystemType(wrappedType, "DateOnly") => LiteDbStorageKind.DateOnlyUtcMidnight,
            _ when IsSystemType(wrappedType, "DateTimeOffset") => LiteDbStorageKind.DateTimeOffsetDocument,
            _ => LiteDbStorageKind.None
        };
    }

    private static bool IsSystemType(ITypeSymbol? symbol, string metadataName) =>
        symbol is INamedTypeSymbol named &&
        named.MetadataName == metadataName &&
        named.ContainingNamespace is { IsGlobalNamespace: false } containingNamespace &&
        containingNamespace.ToDisplayString() == "System";
}

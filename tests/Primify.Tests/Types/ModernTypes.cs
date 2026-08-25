using Primify.Attributes;

namespace Primify.Generator.Tests.Types;

// Full primitive matrix for cross-cutting serialization/LiteDB roundtrip coverage.
[Primify<bool>]
public readonly partial record struct BoolValue;

[Primify<byte>]
public readonly partial record struct ByteValue;

[Primify<sbyte>]
public readonly partial record struct SByteValue;

[Primify<short>]
public readonly partial record struct ShortValue;

[Primify<ushort>]
public readonly partial record struct UShortValue;

[Primify<uint>]
public readonly partial record struct UIntValue;

[Primify<long>]
public readonly partial record struct LongValue;

[Primify<ulong>]
public readonly partial record struct ULongValue;

[Primify<float>]
public readonly partial record struct FloatValue;

[Primify<double>]
public readonly partial record struct DoubleValue;

[Primify<decimal>]
public readonly partial record struct DecimalValue;

[Primify<char>]
public readonly partial record struct CharValue;

[Primify<Guid>]
public readonly partial record struct GuidValue;

[Primify<DateTime>]
public readonly partial record struct DateTimeValue;

[Primify<TimeSpan>]
public readonly partial record struct TimeSpanValue;

[Primify<DateOnly>]
public readonly partial record struct DateOnlyValue;

[Primify<TimeOnly>]
public readonly partial record struct TimeOnlyValue;

[Primify<DateTimeOffset>]
public readonly partial record struct DateTimeOffsetValue;

[Primify<Half>]
public readonly partial record struct HalfValue;

namespace Primify.Converters;

using System;
using System.Globalization;
using LiteDB;

/// <summary>
/// Registers BSON serialize/deserialize mappings for Primify wrapper types.
/// Bridges modern .NET primitives onto the types LiteDB natively stores
/// (Int32, Int64, Double, Decimal, String, Boolean, Guid, DateTime, Binary),
/// converting back on read. One mapping per wrapper type; registering again replaces.
/// </summary>
public static class LiteDbMapping
{
    /// <summary>
    /// Registers a mapping for <typeparamref name="TWrapper"/> on the given mapper (defaults to
    /// <see cref="BsonMapper.Global"/>). Values are converted to a LiteDB-supported BSON type on
    /// write and rebuilt through <see cref="IPrimify{TSelf, TValue}.From"/> on read, so validation
    /// and normalization run during deserialization.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Registering the same wrapper type again replaces the previous mapping. The generated
    /// <c>[ModuleInitializer]</c> calls this method on <see cref="BsonMapper.Global"/> at startup;
    /// call it again with a custom <paramref name="mapper"/> or re-register afterwards to override.
    /// </para>
    /// <para>
    /// Supported values of <typeparamref name="TValue"/>: <c>string</c>, <c>bool</c>,
    /// <c>int</c>, <c>long</c>, <c>double</c>, <c>decimal</c>, <c>Guid</c>, <c>DateTime</c> (stored
    /// as UTC instants, millisecond precision), <c>byte</c>, <c>sbyte</c>, <c>short</c>,
    /// <c>ushort</c>, <c>char</c> (Int32), <c>uint</c>/<c>ulong</c> (Int64; overflow throws),
    /// <c>float</c>/<c>Half</c> (Double), <c>TimeSpan</c>/<c>TimeOnly</c> (Int64 ticks),
    /// <c>DateOnly</c> (UTC-midnight DateTime), and <c>DateTimeOffset</c> (document
    /// <c>{DateTime, Offset}</c>; offset preserved exactly).
    /// </para>
    /// </remarks>
    /// <exception cref="NotSupportedException">
    /// <typeparamref name="TValue"/> has no built-in bridge. Wrap a supported primitive instead, or
    /// register a custom mapping directly on the mapper after startup.
    /// </exception>
    /// <example>
    /// <code>
    /// // Override the default mapping for one wrapper on a private mapper:
    /// var mapper = new BsonMapper();
    /// LiteDbMapping.Register&lt;OrderId, Guid&gt;(mapper);
    ///
    /// using var db = new LiteDatabase("orders.db", mapper);
    /// </code>
    /// </example>
    public static void Register<TWrapper, TValue>(BsonMapper? mapper = null)
        where TWrapper : IPrimify<TWrapper, TValue>
    {
        var target = mapper ?? BsonMapper.Global;

        var (write, read) = Resolve<TValue>();

        target.RegisterType<TWrapper>(
            serialize: wrapper => write(wrapper.Value!),
            deserialize: bson => TWrapper.From((TValue)read(bson)));
    }

    private static (Func<object, BsonValue> Write, Func<BsonValue, object> Read) Resolve<TValue>()
    {
        var type = typeof(TValue);

        if (type == typeof(string))
        {
            return (
                value => new BsonValue((string)value),
                bson => bson.AsString);
        }

        if (type == typeof(bool))
        {
            return (
                value => new BsonValue((bool)value),
                bson => bson.AsBoolean);
        }

        if (type == typeof(int))
        {
            return (
                value => new BsonValue((int)value),
                bson => bson.AsInt32);
        }

        if (type == typeof(long))
        {
            return (
                value => new BsonValue((long)value),
                bson => bson.AsInt64);
        }

        if (type == typeof(double))
        {
            return (
                value => new BsonValue((double)value),
                bson => bson.AsDouble);
        }

        if (type == typeof(decimal))
        {
            return (
                value => new BsonValue((decimal)value),
                bson => bson.AsDecimal);
        }

        if (type == typeof(Guid))
        {
            return (
                value => new BsonValue((Guid)value),
                bson => bson.AsGuid);
        }

        if (type == typeof(DateTime))
        {
            return (
                value => new BsonValue(((DateTime)value).ToUniversalTime()),
                bson => bson.AsDateTime.ToUniversalTime());
        }

        // Narrow integral types widen to Int32; checked casts restore range safety on read.
        if (type == typeof(byte))
        {
            return (
                value => new BsonValue((int)(byte)value),
                bson => checked((byte)bson.AsInt32));
        }

        if (type == typeof(sbyte))
        {
            return (
                value => new BsonValue((int)(sbyte)value),
                bson => checked((sbyte)bson.AsInt32));
        }

        if (type == typeof(short))
        {
            return (
                value => new BsonValue((int)(short)value),
                bson => checked((short)bson.AsInt32));
        }

        if (type == typeof(ushort))
        {
            return (
                value => new BsonValue((int)(ushort)value),
                bson => checked((ushort)bson.AsInt32));
        }

        if (type == typeof(char))
        {
            return (
                value => new BsonValue((int)(char)value),
                bson => checked((char)bson.AsInt32));
        }

        // uint widens to Int64 following LiteDB's own UInt32 -> Int64 convention.
        if (type == typeof(uint))
        {
            return (
                value => new BsonValue((long)(uint)value),
                bson => checked((uint)bson.AsInt64));
        }

        // ulong has no lossless Int64 domain edge at long.MaxValue; overflow throws instead of wrapping.
        if (type == typeof(ulong))
        {
            return (
                value => new BsonValue(checked((long)(ulong)value)),
                bson => checked((ulong)bson.AsInt64));
        }

        if (type == typeof(float))
        {
            return (
                value => new BsonValue((double)(float)value),
                bson => (float)bson.AsDouble);
        }

        if (type == typeof(Half))
        {
            return (
                value => new BsonValue((double)(Half)value),
                bson => (Half)bson.AsDouble);
        }

        if (type == typeof(TimeSpan))
        {
            return (
                value => new BsonValue(((TimeSpan)value).Ticks),
                bson => new TimeSpan(bson.AsInt64));
        }

        if (type == typeof(DateOnly))
        {
            return (
                value => new BsonValue(System.DateTime.SpecifyKind(((DateOnly)value).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)),
                bson => DateOnly.FromDateTime(bson.AsDateTime.ToUniversalTime()));
        }

        if (type == typeof(TimeOnly))
        {
            return (
                value => new BsonValue(((TimeOnly)value).Ticks),
                bson => new TimeOnly(bson.AsInt64));
        }

        // Offset survives via a companion field. The UTC component is truncated to milliseconds
        // by the LiteDB storage engine itself; the offset is preserved exactly.
        if (type == typeof(DateTimeOffset))
        {
            return (
                value =>
                {
                    var dto = (DateTimeOffset)value;
                    return new BsonDocument
                    {
                        ["DateTime"] = new BsonValue(dto.UtcDateTime),
                        ["Offset"] = new BsonValue(dto.Offset.Ticks),
                    };
                },
                bson =>
                {
                    var doc = bson.AsDocument;
                    var utcTicks = doc["DateTime"].AsDateTime.ToUniversalTime();
                    var utc = new DateTimeOffset(System.DateTime.SpecifyKind(utcTicks, DateTimeKind.Utc));
                    return utc.ToOffset(new TimeSpan(doc["Offset"].AsInt64));
                });
        }

        throw new NotSupportedException(
            $"Primify does not provide a built-in LiteDB mapping for '{typeof(TValue)}'. " +
            $"Register one manually: mapper.RegisterType<{typeof(TValue)}>(...), or wrap a supported primitive.");
    }
}

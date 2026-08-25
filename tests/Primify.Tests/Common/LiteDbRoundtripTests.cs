namespace Primify.Generator.Tests.Common;

using LiteDB;

public class LiteDbRoundtripTests
{
    private sealed class Holder<T>
    {
        public int Id { get; set; }

#pragma warning disable CS8618
        public T Value { get; set; }
#pragma warning restore CS8618
    }

    private static async Task RoundTrips<T>(T value, Action<T>? postAssert = null) where T : IEquatable<T>
    {
        using var db = new LiteDatabase(":memory:");
        var collection = db.GetCollection<Holder<T>>($"rt_{typeof(T).Name}");

        collection.Insert(new Holder<T> { Id = 7, Value = value });

        var found = collection.FindById(7);
        await Assert.That(found).IsNotNull();
        await Assert.That(found!.Value).IsEqualTo(value);
        if (postAssert is not null)
        {
            postAssert(value);
        }

        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(Holder<T>), "x");
        var member = System.Linq.Expressions.Expression.Property(parameter, nameof(Holder<T>.Value));
        var constant = System.Linq.Expressions.Expression.Constant(value, typeof(T));
        var predicate = System.Linq.Expressions.Expression.Lambda<Func<Holder<T>, bool>>(
            System.Linq.Expressions.Expression.Equal(member, constant), parameter);

        var queried = collection.FindOne(predicate);
        await Assert.That(queried).IsNotNull();
    }

    private static DateTime MillisecondUtc(int year, int month, int day, int hour, int minute, int second)
        => new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);

    [Test]
    public async Task Bool_RoundTrips()
    {
        await RoundTrips(BoolValue.From(true));
    }

    [Test]
    public async Task Byte_RoundTrips()
    {
        await RoundTrips(ByteValue.From(200));
    }

    [Test]
    public async Task SByte_RoundTrips()
    {
        await RoundTrips(SByteValue.From(-100));
    }

    [Test]
    public async Task Short_RoundTrips()
    {
        await RoundTrips(ShortValue.From(-30_000));
    }

    [Test]
    public async Task UShort_RoundTrips()
    {
        await RoundTrips(UShortValue.From(60_000));
    }

    [Test]
    public async Task UInt_RoundTrips()
    {
        await RoundTrips(UIntValue.From(4_000_000_000u));
    }

    [Test]
    public async Task Int_RoundTrips()
    {
        await RoundTrips(IntRecordStruct.From(-42));
    }

    [Test]
    public async Task Long_RoundTrips()
    {
        await RoundTrips(LongValue.From(-9_000_000_000L));
    }

    [Test]
    public async Task ULong_RoundTrips()
    {
        await RoundTrips(ULongValue.From(9_000_000_000ul));
    }

    [Test]
    public async Task Float_RoundTrips()
    {
        await RoundTrips(FloatValue.From(1.5f));
    }

    [Test]
    public async Task Double_RoundTrips()
    {
        await RoundTrips(DoubleValue.From(199.88d));
    }

    [Test]
    public async Task Decimal_RoundTrips()
    {
        await RoundTrips(DecimalValue.From(1234.56m));
    }

    [Test]
    public async Task Char_RoundTrips()
    {
        await RoundTrips(CharValue.From('X'));
    }

    [Test]
    public async Task String_RoundTrips()
    {
        await RoundTrips(StringRecordStruct.From("hello"));
    }

    [Test]
    public async Task Guid_RoundTrips()
    {
        await RoundTrips(GuidValue.From(Guid.NewGuid()));
    }

    [Test]
    public async Task DateTime_RoundTrips_MillisecondPrecision()
    {
        await RoundTrips(DateTimeValue.From(MillisecondUtc(2026, 8, 25, 13, 45, 59)));
    }

    [Test]
    public async Task TimeSpan_RoundTrips()
    {
        await RoundTrips(TimeSpanValue.From(new TimeSpan(1, 2, 3, 4, 567)));
    }

    [Test]
    public async Task DateOnly_RoundTrips()
    {
        await RoundTrips(DateOnlyValue.From(new DateOnly(2026, 8, 25)));
    }

    [Test]
    public async Task TimeOnly_RoundTrips()
    {
        await RoundTrips(TimeOnlyValue.From(new TimeOnly(13, 45, 59, 123)));
    }

    [Test]
    public async Task Half_RoundTrips()
    {
        await RoundTrips(HalfValue.From((Half)2.5f));
    }

    [Test]
    public async Task DateTimeOffset_Preserves_Offset()
    {
        var value = new DateTimeOffset(2026, 8, 25, 13, 45, 59, TimeSpan.FromHours(5.5));

        await RoundTrips(DateTimeOffsetValue.From(value));
    }

    [Test]
    public async Task ULong_Beyond_LongRange_Throws_OnSerialize()
    {
        using var db = new LiteDatabase(":memory:");
        var collection = db.GetCollection<Holder<ULongValue>>("rt_overflow");

        await Assert.ThrowsAsync<OverflowException>(async () =>
            _ = collection.Insert(new Holder<ULongValue> { Id = 1, Value = ULongValue.From(ulong.MaxValue) }));
    }

    [Test]
    public async Task WrappedType_InDocument_Supports_Update_And_Count()
    {
        using var db = new LiteDatabase(":memory:");
        var collection = db.GetCollection<Holder<LongValue>>("rt_update");

        var doc = new Holder<LongValue> { Id = 3, Value = LongValue.From(10) };
        collection.Insert(doc);

        doc.Value = LongValue.From(20);
        await Assert.That(collection.Update(doc)).IsTrue();

        var updated = collection.FindById(3);
        await Assert.That(updated!.Value).IsEqualTo(LongValue.From(20));

        await Assert.That(collection.Count(_ => true)).IsEqualTo(1);
    }
}

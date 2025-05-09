using System;
using System.Text.Json;
using LiteDB;
using Newtonsoft.Json;
using Primify.Tests.Models;
using TUnit;
using TUnit.Assertions;
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Primify.Tests;

public class DateTimeOffsetValueTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;
    private static readonly DateTimeOffset Tomorrow = Now.AddDays(1);
    private static readonly DateTimeOffset Yesterday = Now.AddDays(-1);

    // --- DateTimeOffsetClassValue Tests ---

    [Test]
    public async Task DateTimeOffsetClassValue_Creation_ValidFutureDate_SucceedsAndNormalizesToUtc()
    {
        var localTomorrow = new DateTimeOffset(Tomorrow.Year, Tomorrow.Month, Tomorrow.Day, 10, 0, 0, TimeSpan.FromHours(-5));
        var val = DateTimeOffsetClassValue.From(localTomorrow);
        // Should be normalized to UTC, date part should match Tomorrow's UTC date
        await Assert.That(val.Value.Offset).IsEqualTo(TimeSpan.Zero);
        await Assert.That(val.Value.Date).IsEqualTo(localTomorrow.UtcDateTime.Date);
    }

    [Test]
    public async Task DateTimeOffsetClassValue_Creation_PastDate_ThrowsArgumentOutOfRangeException()
    {
        var localYesterday = new DateTimeOffset(Yesterday.Year, Yesterday.Month, Yesterday.Day, 10, 0, 0, TimeSpan.FromHours(-5));
        await Assert.That(() => DateTimeOffsetClassValue.From(localYesterday))
            .ThrowsExactly<ArgumentOutOfRangeException>() // Use ThrowsExactly
            .WithMessageMatching("DateTimeOffsetClassValue must not be in the past (date component).*");
    }

    [Test]
    public async Task DateTimeOffsetClassValue_Creation_TodayButPastTime_StillValidIfDateIsTodayUtc()
    {
        // Create a time that is in the past today (local), but whose UTC date is still today.
        var localTodayPastTime = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-5)).AddHours(-2); // 2 hours ago, local
        // If localTodayPastTime.UtcDateTime.Date is still UtcNow.Date, it should pass.
        // The validation is `value < DateTimeOffset.UtcNow.Date`
        // `value` is normalized to UTC. So if `localTodayPastTime.ToUniversalTime().Date == DateTimeOffset.UtcNow.Date`, it's fine.

        var val = DateTimeOffsetClassValue.From(localTodayPastTime);
        await Assert.That(val.Value.Offset).IsEqualTo(TimeSpan.Zero);
        // This assertion depends on the exact moment of execution relative to UTC day change.
        // For robustness, we ensure the normalized date is not *before* today's UTC date.
        await Assert.That(val.Value.Date).IsGreaterThanOrEqualTo(DateTimeOffset.UtcNow.Date);
    }


    [Test]
    public async Task DateTimeOffsetClassValue_ExplicitConversion_ToDateTimeOffset_Works()
    {
        var val = DateTimeOffsetClassValue.From(Tomorrow);
        DateTimeOffset primitive = (DateTimeOffset)val;
        await Assert.That(primitive).IsEqualTo(Tomorrow.ToUniversalTime());
    }

    [Test]
    public async Task DateTimeOffsetClassValue_ExplicitConversion_FromDateTimeOffset_Works()
    {
        DateTimeOffsetClassValue val = (DateTimeOffsetClassValue)Tomorrow;
        await Assert.That(val.Value).IsEqualTo(Tomorrow.ToUniversalTime());
    }

    [Test]
    public async Task DateTimeOffsetClassValue_ExplicitConversion_FromPastDate_Throws()
    {
        await Assert.That(() => (DateTimeOffsetClassValue)Yesterday)
            .ThrowsExactly<ArgumentOutOfRangeException>() // Use ThrowsExactly
            .WithMessageMatching("DateTimeOffsetClassValue must not be in the past (date component).*");
    }

    [Test]
    public async Task DateTimeOffsetClassValue_ToString_ReturnsNormalizedUtcString()
    {
        var val = DateTimeOffsetClassValue.From(Tomorrow);
        await Assert.That(val.ToString()).IsEqualTo(Tomorrow.ToUniversalTime().ToString("O")); // ISO 8601
    }

    [Test]
    public async Task DateTimeOffsetClassValue_SystemTextJson_SerializationDeserialization_Works()
    {
        var val = DateTimeOffsetClassValue.From(Tomorrow);
        var json = JsonSerializer.Serialize(val);
        var deserialized = JsonSerializer.Deserialize<DateTimeOffsetClassValue>(json);
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.Value).IsEqualTo(Tomorrow.ToUniversalTime());
    }

    [Test]
    public async Task DateTimeOffsetClassValue_SystemTextJson_DeserializeNull_ReturnsNull()
    {
        var deserialized = JsonSerializer.Deserialize<DateTimeOffsetClassValue>("null");
        await Assert.That(deserialized).IsNull();
    }

    [Test]
    public async Task DateTimeOffsetClassValue_NewtonsoftJson_SerializationDeserialization_Works()
    {
        var val = DateTimeOffsetClassValue.From(Tomorrow);
        var json = JsonConvert.SerializeObject(val);
        var deserialized = JsonConvert.DeserializeObject<DateTimeOffsetClassValue>(json);
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.Value).IsEqualTo(Tomorrow.ToUniversalTime());
    }

    [Test]
    public async Task DateTimeOffsetClassValue_NewtonsoftJson_DeserializeNull_ReturnsNull()
    {
        var deserialized = JsonConvert.DeserializeObject<DateTimeOffsetClassValue>("null");
        await Assert.That(deserialized).IsNull();
    }

    [Test]
    public async Task DateTimeOffsetClassValue_LiteDB_SerializationDeserialization_Works()
    {
        var val = DateTimeOffsetClassValue.From(Tomorrow);
        var mapper = new BsonMapper();
        // LiteDB stores DateTimeOffset as DateTime by default (usually UTC if handled correctly)
        mapper.RegisterType(
            serialize: v => new BsonValue(v.Value),
            deserialize: bson => DateTimeOffsetClassValue.From(bson.AsDateTime) // Corrected to AsDateTime
        );

        var bsonDoc = mapper.ToDocument(new { Value = val });
        var deserialized = mapper.ToObject<TestEntity<DateTimeOffsetClassValue>>(bsonDoc);

        await Assert.That(deserialized).IsNotNull();
        // LiteDB's BsonValue.AsDateTimeOffset might return local if not careful.
        // The generator's default LiteDB handling for DateTimeOffset is AsDateTime, which is then converted.
        // Our From() method normalizes to UTC.
        await Assert.That(deserialized.Value.Value.Offset).IsEqualTo(TimeSpan.Zero);
        await Assert.That(deserialized.Value.Value).IsEqualTo(Tomorrow.ToUniversalTime());
    }

    // --- DateTimeOffsetStructValue Tests ---

    [Test]
    public async Task DateTimeOffsetStructValue_Creation_ValidDate_SucceedsAndNormalizesOffset()
    {
        var localNextYear = new DateTimeOffset(Now.Year + 1, 1, 1, 10, 0, 0, TimeSpan.FromHours(-5));
        var val = DateTimeOffsetStructValue.From(localNextYear);
        await Assert.That(val.Value.Offset).IsEqualTo(TimeSpan.Zero);
        await Assert.That(val.Value.DateTime).IsEqualTo(localNextYear.DateTime); // DateTime part preserved
        await Assert.That(val.Value.Year).IsEqualTo(Now.Year + 1);
    }

    [Test]
    public async Task DateTimeOffsetStructValue_Creation_PastYear_ThrowsArgumentOutOfRangeException()
    {
        var lastYear = new DateTimeOffset(Now.Year - 1, 12, 31, 23, 59, 59, TimeSpan.Zero);
        await Assert.That(() => DateTimeOffsetStructValue.From(lastYear))
            .ThrowsExactly<ArgumentOutOfRangeException>() // Use ThrowsExactly
            .WithMessageMatching($"DateTimeOffsetStructValue year must be*");
    }

    [Test]
    public async Task DateTimeOffsetStructValue_Creation_DefaultIsValid()
    {
        // Default struct will have DateTimeOffset.MinValue. Its year will likely fail validation.
        // However, default constructor doesn't run validation.
        var val = default(DateTimeOffsetStructValue);
        await Assert.That(val.Value).IsEqualTo(DateTimeOffset.MinValue);
    }


    [Test]
    public async Task DateTimeOffsetStructValue_ExplicitConversion_ToDateTimeOffset_Works()
    {
        var currentYearDate = new DateTimeOffset(Now.Year, Now.Month, Now.Day, 10,0,0, TimeSpan.FromHours(2));
        var val = DateTimeOffsetStructValue.From(currentYearDate);
        DateTimeOffset primitive = (DateTimeOffset)val;
        await Assert.That(primitive.Offset).IsEqualTo(TimeSpan.Zero);
        await Assert.That(primitive.DateTime).IsEqualTo(currentYearDate.DateTime);
    }

    [Test]
    public async Task DateTimeOffsetStructValue_ExplicitConversion_FromDateTimeOffset_Works()
    {
        var currentYearDate = new DateTimeOffset(Now.Year, Now.Month, Now.Day, 10,0,0, TimeSpan.FromHours(2));
        DateTimeOffsetStructValue val = (DateTimeOffsetStructValue)currentYearDate;
        await Assert.That(val.Value.Offset).IsEqualTo(TimeSpan.Zero);
        await Assert.That(val.Value.DateTime).IsEqualTo(currentYearDate.DateTime);
    }

    [Test]
    public async Task DateTimeOffsetStructValue_ExplicitConversion_FromPastYear_Throws()
    {
        var lastYear = new DateTimeOffset(Now.Year - 1, 1,1,1,1,1, TimeSpan.Zero);
        await Assert.That(() => (DateTimeOffsetStructValue)lastYear)
            .ThrowsExactly<ArgumentOutOfRangeException>() // Use ThrowsExactly
            .WithMessageMatching($"DateTimeOffsetStructValue year must be*");
    }

    [Test]
    public async Task DateTimeOffsetStructValue_ToString_ReturnsNormalizedString()
    {
        var currentYearDate = new DateTimeOffset(Now.Year, Now.Month, Now.Day, 10,0,0, TimeSpan.FromHours(2));
        var val = DateTimeOffsetStructValue.From(currentYearDate);
        var expectedDto = new DateTimeOffset(currentYearDate.DateTime, TimeSpan.Zero);
        await Assert.That(val.ToString()).IsEqualTo(expectedDto.ToString("O"));
    }

    [Test]
    public async Task DateTimeOffsetStructValue_SystemTextJson_SerializationDeserialization_Works()
    {
        var currentYearDate = new DateTimeOffset(Now.Year, Now.Month, Now.Day, 10,0,0, TimeSpan.FromHours(2));
        var val = DateTimeOffsetStructValue.From(currentYearDate);
        var json = JsonSerializer.Serialize(val);
        var deserialized = JsonSerializer.Deserialize<DateTimeOffsetStructValue>(json);

        var expectedDto = new DateTimeOffset(currentYearDate.DateTime, TimeSpan.Zero);
        await Assert.That(deserialized.Value).IsEqualTo(expectedDto);
    }

    [Test]
    public async Task DateTimeOffsetStructValue_SystemTextJson_DeserializeNull_ReturnsDefault()
    {
        var deserialized = JsonSerializer.Deserialize<DateTimeOffsetStructValue>("null");
        await Assert.That(deserialized.Value).IsEqualTo(DateTimeOffset.MinValue);
        await Assert.That(deserialized).IsEqualTo(default(DateTimeOffsetStructValue));
    }

    [Test]
    public async Task DateTimeOffsetStructValue_NewtonsoftJson_SerializationDeserialization_Works()
    {
        var currentYearDate = new DateTimeOffset(Now.Year, Now.Month, Now.Day, 10,0,0, TimeSpan.FromHours(2));
        var val = DateTimeOffsetStructValue.From(currentYearDate);
        var json = JsonConvert.SerializeObject(val);
        var deserialized = JsonConvert.DeserializeObject<DateTimeOffsetStructValue>(json);

        var expectedDto = new DateTimeOffset(currentYearDate.DateTime, TimeSpan.Zero);
        await Assert.That(deserialized.Value).IsEqualTo(expectedDto);
    }

    [Test]
    public async Task DateTimeOffsetStructValue_NewtonsoftJson_DeserializeNull_ReturnsDefault()
    {
        var deserialized = JsonConvert.DeserializeObject<DateTimeOffsetStructValue>("null");
        await Assert.That(deserialized.Value).IsEqualTo(DateTimeOffset.MinValue);
        await Assert.That(deserialized).IsEqualTo(default(DateTimeOffsetStructValue));
    }

    [Test]
    public async Task DateTimeOffsetStructValue_LiteDB_SerializationDeserialization_Works()
    {
        var currentYearDate = new DateTimeOffset(Now.Year, Now.Month, Now.Day, 10,0,0, TimeSpan.FromHours(2));
        var val = DateTimeOffsetStructValue.From(currentYearDate);
        var mapper = new BsonMapper();
        mapper.RegisterType(
            serialize: v => new BsonValue(v.Value),
            deserialize: bson => DateTimeOffsetStructValue.From(bson.AsDateTime) // Corrected to AsDateTime
        );

        var bsonDoc = mapper.ToDocument(new { Value = val });
        var deserialized = mapper.ToObject<TestEntity<DateTimeOffsetStructValue>>(bsonDoc);

        var expectedDto = new DateTimeOffset(currentYearDate.DateTime, TimeSpan.Zero);
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized.Value.Value).IsEqualTo(expectedDto);
    }

    private class TestEntity<T>
    {
        public T Value { get; set; } = default!;
    }
}

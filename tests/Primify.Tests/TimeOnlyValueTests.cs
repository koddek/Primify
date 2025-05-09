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

public class TimeOnlyValueTests
{
    private static readonly TimeOnly ValidTime = new(10, 30, 45); // 10:30:45 AM
    private static readonly TimeOnly ValidTimeNormalizedClass = new(10, 30, 0); // Normalized for ClassValue
    private static readonly TimeOnly ValidTimeNormalizedStruct = new(10, 30, 45); // Normalized for StructValue (seconds kept)

    private static readonly TimeOnly InvalidTimeClassLow = new(8, 59, 59); // Before 9 AM
    private static readonly TimeOnly InvalidTimeClassHigh = new(17, 0, 1);  // After 5 PM (normalized to 17:00, but original is > 17:00)
    private static readonly TimeOnly InvalidTimeStructMillis = new (10,30,45, 123); // Has milliseconds

    // --- TimeOnlyClassValue Tests ---

    [Test]
    public async Task TimeOnlyClassValue_Creation_ValidTime_SucceedsAndNormalizes()
    {
        var val = TimeOnlyClassValue.From(ValidTime);
        await Assert.That(val.Value).IsEqualTo(ValidTimeNormalizedClass);
    }

    [Test]
    public async Task TimeOnlyClassValue_Creation_InvalidTimeLow_ThrowsArgumentOutOfRangeException()
    {
        await Assert.That(() => TimeOnlyClassValue.From(InvalidTimeClassLow))
            .ThrowsExactly<ArgumentOutOfRangeException>() // Use ThrowsExactly
            .WithMessageMatching("TimeOnlyClassValue must be between 9 AM and 5 PM (inclusive).*");
    }

    [Test]
    public async Task TimeOnlyClassValue_Creation_InvalidTimeHigh_ThrowsArgumentOutOfRangeException()
    {
        // Note: 17:00:01 normalizes to 17:00:00, which is valid by the check.
        // The validation happens on the *normalized* value.
        // To test boundary, we need something that normalizes to outside the range, or is already outside.
        var justOverFivePm = new TimeOnly(17, 1, 0); // 5:01 PM
        await Assert.That(() => TimeOnlyClassValue.From(justOverFivePm))
            .ThrowsExactly<ArgumentOutOfRangeException>() // Use ThrowsExactly
            .WithMessageMatching("TimeOnlyClassValue must be between 9 AM and 5 PM (inclusive).*");
    }


    [Test]
    public async Task TimeOnlyClassValue_ExplicitConversion_ToTimeOnly_Works()
    {
        var val = TimeOnlyClassValue.From(ValidTime);
        TimeOnly primitive = (TimeOnly)val;
        await Assert.That(primitive).IsEqualTo(ValidTimeNormalizedClass);
    }

    [Test]
    public async Task TimeOnlyClassValue_ExplicitConversion_FromTimeOnly_Works()
    {
        TimeOnlyClassValue val = (TimeOnlyClassValue)ValidTime;
        await Assert.That(val.Value).IsEqualTo(ValidTimeNormalizedClass);
    }

    [Test]
    public async Task TimeOnlyClassValue_ExplicitConversion_FromInvalidTime_Throws()
    {
        var justOverFivePm = new TimeOnly(17, 1, 0);
        await Assert.That(() => (TimeOnlyClassValue)justOverFivePm)
            .ThrowsExactly<ArgumentOutOfRangeException>() // Use ThrowsExactly
            .WithMessageMatching("TimeOnlyClassValue must be between 9 AM and 5 PM (inclusive).*");
    }

    [Test]
    public async Task TimeOnlyClassValue_ToString_ReturnsTimeOnlyString()
    {
        var val = TimeOnlyClassValue.From(ValidTime);
        await Assert.That(val.ToString()).IsEqualTo(ValidTimeNormalizedClass.ToString("O")); // ISO 8601 standard format
    }

    [Test]
    public async Task TimeOnlyClassValue_SystemTextJson_SerializationDeserialization_Works()
    {
        var val = TimeOnlyClassValue.From(ValidTime);
        var json = JsonSerializer.Serialize(val);
        var deserialized = JsonSerializer.Deserialize<TimeOnlyClassValue>(json);
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.Value).IsEqualTo(ValidTimeNormalizedClass);
    }

    [Test]
    public async Task TimeOnlyClassValue_SystemTextJson_DeserializeNull_ReturnsNull()
    {
        var deserialized = JsonSerializer.Deserialize<TimeOnlyClassValue>("null");
        await Assert.That(deserialized).IsNull();
    }

    [Test]
    public async Task TimeOnlyClassValue_NewtonsoftJson_SerializationDeserialization_Works()
    {
        var val = TimeOnlyClassValue.From(ValidTime);
        var json = JsonConvert.SerializeObject(val);
        var deserialized = JsonConvert.DeserializeObject<TimeOnlyClassValue>(json);
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.Value).IsEqualTo(ValidTimeNormalizedClass);
    }

    [Test]
    public async Task TimeOnlyClassValue_NewtonsoftJson_DeserializeNull_ReturnsNull()
    {
        var deserialized = JsonConvert.DeserializeObject<TimeOnlyClassValue>("null");
        await Assert.That(deserialized).IsNull();
    }

    [Test]
    public async Task TimeOnlyClassValue_LiteDB_SerializationDeserialization_Works()
    {
        var val = TimeOnlyClassValue.From(ValidTime);
        var mapper = new BsonMapper();
        mapper.RegisterType(
            serialize: v => new BsonValue(v.Value.Ticks), // Store TimeOnly as Ticks (long)
            deserialize: bson => TimeOnlyClassValue.From(TimeOnly.FromTimeSpan(TimeSpan.FromTicks(bson.AsInt64)))
        );

        var bsonDoc = mapper.ToDocument(new { Value = val });
        var deserialized = mapper.ToObject<TestEntity<TimeOnlyClassValue>>(bsonDoc);

        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized.Value.Value).IsEqualTo(ValidTimeNormalizedClass);
    }

    // --- TimeOnlyStructValue Tests ---

    [Test]
    public async Task TimeOnlyStructValue_Creation_ValidTime_SucceedsAndNormalizes()
    {
        var val = TimeOnlyStructValue.From(new TimeOnly(10,30,45,500)); // include millis for normalization test
        await Assert.That(val.Value).IsEqualTo(new TimeOnly(10,30,45,0)); // Normalized
    }

    [Test]
    public async Task TimeOnlyStructValue_Creation_InvalidTimeMillis_ThrowsArgumentException()
    {
        await Assert.That(() => TimeOnlyStructValue.From(InvalidTimeStructMillis))
            .ThrowsExactly<ArgumentException>() // Use ThrowsExactly
            .WithMessageMatching("TimeOnlyStructValue milliseconds must be zero*");
    }

    [Test]
    public async Task TimeOnlyStructValue_Creation_DefaultIsValid()
    {
        // Default struct will have TimeOnly.MinValue, which has 0 milliseconds.
        var val = default(TimeOnlyStructValue);
        await Assert.That(val.Value).IsEqualTo(TimeOnly.MinValue); // Default constructor doesn't run validation
    }

    [Test]
    public async Task TimeOnlyStructValue_ExplicitConversion_ToTimeOnly_Works()
    {
        var val = TimeOnlyStructValue.From(ValidTime); // ValidTime has 0 millis after normalization
        TimeOnly primitive = (TimeOnly)val;
        await Assert.That(primitive).IsEqualTo(ValidTimeNormalizedStruct);
    }

    [Test]
    public async Task TimeOnlyStructValue_ExplicitConversion_FromTimeOnly_Works()
    {
        TimeOnlyStructValue val = (TimeOnlyStructValue)ValidTime; // ValidTime has 0 millis
        await Assert.That(val.Value).IsEqualTo(ValidTimeNormalizedStruct);
    }

    [Test]
    public async Task TimeOnlyStructValue_ExplicitConversion_FromInvalidTime_Throws()
    {
        await Assert.That(() => (TimeOnlyStructValue)InvalidTimeStructMillis)
            .ThrowsExactly<ArgumentException>() // Use ThrowsExactly
            .WithMessageMatching("TimeOnlyStructValue milliseconds must be zero*");
    }

    [Test]
    public async Task TimeOnlyStructValue_ToString_ReturnsTimeOnlyString()
    {
        var val = TimeOnlyStructValue.From(ValidTime);
        await Assert.That(val.ToString()).IsEqualTo(ValidTimeNormalizedStruct.ToString("O"));
    }

    [Test]
    public async Task TimeOnlyStructValue_SystemTextJson_SerializationDeserialization_Works()
    {
        var val = TimeOnlyStructValue.From(ValidTime);
        var json = JsonSerializer.Serialize(val);
        var deserialized = JsonSerializer.Deserialize<TimeOnlyStructValue>(json);
        await Assert.That(deserialized.Value).IsEqualTo(ValidTimeNormalizedStruct);
    }

    [Test]
    public async Task TimeOnlyStructValue_SystemTextJson_DeserializeNull_ReturnsDefault()
    {
        var deserialized = JsonSerializer.Deserialize<TimeOnlyStructValue>("null");
        await Assert.That(deserialized.Value).IsEqualTo(TimeOnly.MinValue);
        await Assert.That(deserialized).IsEqualTo(default(TimeOnlyStructValue));
    }

    [Test]
    public async Task TimeOnlyStructValue_NewtonsoftJson_SerializationDeserialization_Works()
    {
        var val = TimeOnlyStructValue.From(ValidTime);
        var json = JsonConvert.SerializeObject(val);
        var deserialized = JsonConvert.DeserializeObject<TimeOnlyStructValue>(json);
        await Assert.That(deserialized.Value).IsEqualTo(ValidTimeNormalizedStruct);
    }

    [Test]
    public async Task TimeOnlyStructValue_NewtonsoftJson_DeserializeNull_ReturnsDefault()
    {
        var deserialized = JsonConvert.DeserializeObject<TimeOnlyStructValue>("null");
        await Assert.That(deserialized.Value).IsEqualTo(TimeOnly.MinValue);
        await Assert.That(deserialized).IsEqualTo(default(TimeOnlyStructValue));
    }

    [Test]
    public async Task TimeOnlyStructValue_LiteDB_SerializationDeserialization_Works()
    {
        var val = TimeOnlyStructValue.From(ValidTime);
        var mapper = new BsonMapper();
        mapper.RegisterType(
            serialize: v => new BsonValue(v.Value.Ticks),
            deserialize: bson => TimeOnlyStructValue.From(TimeOnly.FromTimeSpan(TimeSpan.FromTicks(bson.AsInt64)))
        );

        var bsonDoc = mapper.ToDocument(new { Value = val });
        var deserialized = mapper.ToObject<TestEntity<TimeOnlyStructValue>>(bsonDoc);

        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized.Value.Value).IsEqualTo(ValidTimeNormalizedStruct);
    }

    private class TestEntity<T>
    {
        public T Value { get; set; } = default!; // Changed from Id to Value to match BsonDoc structure
    }
}

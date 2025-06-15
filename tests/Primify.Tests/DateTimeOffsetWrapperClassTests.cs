namespace Primify.Generator.Tests;

[Primify<DateTimeOffset>]
public partial class DateTimeOffsetPrimowrapClass;

// [Primify<DateTimeOffset>]
// public partial class DateTimeOffsetWrapperClassWithNormalize
// {
//     private static DateTimeOffset Normalize(DateTimeOffset value) => value < 1 ? -1 : value;
// }

[Primify<DateTimeOffset>]
public partial class DateTimeOffsetPrimowrapClassWithPredefinedProperty
{
    public static DateTimeOffsetPrimowrapClassWithPredefinedProperty Empty => new(DateTimeOffset.MinValue);
}

public class DateTimeOffsetWrapperClassTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void DateTimeOffsetWrapperClass_CreatesType_WhenFromIsCalled()
    {
        // Arrange
        var expectedValue = DateTimeOffset.MinValue;

        // Act
        var result = DateTimeOffsetPrimowrapClass.From(expectedValue);
        testOutputHelper.WriteLine(result.ToString());

        // Assert
        Assert.Equal(expectedValue, result.Value);
        Assert.Equal(expectedValue, result.Value);
    }

    [Fact]
    public void DateTimeOffsetWrapperClass_CreatesType_WhenExplicitlyCastToFromType()
    {
        var expectedValue = DateTimeOffset.MinValue;

        var result1 = (DateTimeOffsetPrimowrapClass)expectedValue;
        var result2 = (DateTimeOffset)result1;
        testOutputHelper.WriteLine(result1.ToString());

        Assert.Equal(expectedValue, result1.Value);
        Assert.Equal(expectedValue, result1.Value);
        Assert.Equal(expectedValue, result2);
    }

    // [Theory]
    // [InlineData(1, 1)]
    // [InlineData(10, 10)]
    // [InlineData(0, -1)]
    // [InlineData(-1, -1)]
    // [InlineData(-100, -1)]
    // public void DateTimeOffsetWrapperClassWithNormalize_ReturnsNormalizedValue_WhenCalledWithNonNormalizedValue(int value, int expected)
    // {
    //     var result = DateTimeOffsetWrapperClassWithNormalize.From(value);
    //     testOutputHelper.WriteLine(result.ToString());
    //
    //     Assert.Equal(expected, result.Value);
    // }

    [Fact]
    public void DateTimeOffsetWrapperClassWithPredefinedProperty_IgnoresReadonly_WhenSerializedWithSystemTextJsonV1()
    {
        var expectedValue = DateTimeOffset.Now;
        var result = DateTimeOffsetPrimowrapClassWithPredefinedProperty.From(expectedValue);

        // Default value to string
        testOutputHelper.WriteLine("result.Value:");
        testOutputHelper.WriteLine(result.Value.ToString());

        Assert.Equal(expectedValue, result.Value);

        // System.Text.Json serialization
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        testOutputHelper.WriteLine("\nSystem.Text.Json serialization:");
        testOutputHelper.WriteLine(json);

        // System.Text.Json deserialization
        var stjDeserialized =
            System.Text.Json.JsonSerializer.Deserialize<DateTimeOffsetPrimowrapClassWithPredefinedProperty>(json);
        testOutputHelper.WriteLine("\nSystem.Text.Json deserialized value:");
        testOutputHelper.WriteLine(stjDeserialized?.ToString() ?? "null");
        Assert.Equal(expectedValue, stjDeserialized?.Value);
    }

    [Fact]
    public void DateTimeOffsetWrapperClassWithPredefinedProperty_IgnoresReadonly_WhenSerializedWithSystemTextJson()
    {
        var expectedValue = DateTimeOffset.MinValue;
        var result = DateTimeOffsetPrimowrapClassWithPredefinedProperty.Empty;

        // Default value to string
        testOutputHelper.WriteLine("result.Value:");
        testOutputHelper.WriteLine(result.Value.ToString());

        Assert.Equal(expectedValue, result.Value);

        // System.Text.Json serialization
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        testOutputHelper.WriteLine("\nSystem.Text.Json serialization:");
        testOutputHelper.WriteLine(json);

        // System.Text.Json deserialization
        var stjDeserialized =
            System.Text.Json.JsonSerializer.Deserialize<DateTimeOffsetPrimowrapClassWithPredefinedProperty>(json);
        testOutputHelper.WriteLine("\nSystem.Text.Json deserialized value:");
        testOutputHelper.WriteLine(stjDeserialized?.ToString() ?? "null");
        Assert.Equal(expectedValue, stjDeserialized?.Value);
    }

    [Fact]
    public void DateTimeOffsetWrapperClassWithPredefinedProperty_IgnoresReadonly_WhenSerializedNewtonsoftJsonV1()
    {
        var expectedValue = DateTimeOffset.Now;
        var result = DateTimeOffsetPrimowrapClassWithPredefinedProperty.From(expectedValue);

        // Default value to string
        testOutputHelper.WriteLine("result.Value:");
        testOutputHelper.WriteLine(result.Value.ToString());

        Assert.Equal(expectedValue, result.Value);

        // Newtonsoft.Json serialization
        var newtonsoftJson = Newtonsoft.Json.JsonConvert.SerializeObject(result);
        testOutputHelper.WriteLine("\nNewtonsoft.Json serialization:");
        testOutputHelper.WriteLine(newtonsoftJson);

        // Newtonsoft.Json deserialization
        var njsDeserialized =
            Newtonsoft.Json.JsonConvert.DeserializeObject<DateTimeOffsetPrimowrapClassWithPredefinedProperty>(
                newtonsoftJson);
        testOutputHelper.WriteLine("\nNewtonsoft.Json deserialized value:");
        testOutputHelper.WriteLine(njsDeserialized?.ToString() ?? "null");
        Assert.Equal(expectedValue, njsDeserialized?.Value);
    }

    [Fact]
    public void DateTimeOffsetWrapperClassWithPredefinedProperty_IgnoresReadonly_WhenSerializedNewtonsoftJson()
    {
        var expectedValue = DateTimeOffset.MinValue;
        var result = DateTimeOffsetPrimowrapClassWithPredefinedProperty.From(expectedValue);

        // Default value to string
        testOutputHelper.WriteLine("result.Value:");
        testOutputHelper.WriteLine(result.Value.ToString());

        Assert.Equal(expectedValue, result.Value);

        // Newtonsoft.Json serialization
        var newtonsoftJson = Newtonsoft.Json.JsonConvert.SerializeObject(result);
        testOutputHelper.WriteLine("\nNewtonsoft.Json serialization:");
        testOutputHelper.WriteLine(newtonsoftJson);

        // Newtonsoft.Json deserialization
        var njsDeserialized =
            Newtonsoft.Json.JsonConvert.DeserializeObject<DateTimeOffsetPrimowrapClassWithPredefinedProperty>(
                newtonsoftJson);
        testOutputHelper.WriteLine("\nNewtonsoft.Json deserialized value:");
        testOutputHelper.WriteLine(njsDeserialized?.ToString() ?? "null");
        Assert.Equal(expectedValue, njsDeserialized?.Value);
    }

    [Fact]
    public void DateTimeOffsetWrapperClassWithPredefinedProperty_IgnoresReadonly_WhenSerializedNewtonsoftBsonV1()
    {
        var expectedValue = DateTimeOffset.UtcNow;
        var result = DateTimeOffsetPrimowrapClassWithPredefinedProperty.From(expectedValue);

        // Default value to string
        testOutputHelper.WriteLine("result.Value:");
        testOutputHelper.WriteLine(result.Value.ToString());

        Assert.Equal(expectedValue, result.Value);

        // BSON serialization
        using var ms = new MemoryStream();
        using (var writer = new Newtonsoft.Json.Bson.BsonDataWriter(ms))
        {
            var serializer = new Newtonsoft.Json.JsonSerializer();
            serializer.Serialize(writer, result);
        }

        var bsonBytes = ms.ToArray();
        var bsonBase64 = Convert.ToBase64String(bsonBytes);
        testOutputHelper.WriteLine("\nBSON serialization (Base64):");
        testOutputHelper.WriteLine(bsonBase64);

        // BSON deserialization
        using var ms2 = new MemoryStream(bsonBytes);
        using (var reader = new Newtonsoft.Json.Bson.BsonDataReader(ms2))
        {
            var serializer = new Newtonsoft.Json.JsonSerializer();
            var bsonDeserialized = serializer.Deserialize<DateTimeOffsetPrimowrapClassWithPredefinedProperty>(reader);
            testOutputHelper.WriteLine("\nBSON deserialized value:");
            testOutputHelper.WriteLine(bsonDeserialized?.ToString() ?? "null");

            // Compare the underlying DateTime values directly to avoid timezone/offset issues
            var expectedDateTime = expectedValue.UtcDateTime;
            var actualDateTime = bsonDeserialized?.Value.UtcDateTime;
            Assert.Equal(expectedDateTime.ToString(), actualDateTime.Value.ToString());
        }
    }

    [Fact]
    public void DateTimeOffsetWrapperClassWithPredefinedProperty_IgnoresReadonly_WhenSerializedNewtonsoftBsonV3()
    {
        var expectedValue = DateTimeOffset.UtcNow;
        var result = DateTimeOffsetPrimowrapClassWithPredefinedProperty.From(expectedValue);

        testOutputHelper.WriteLine($"Original Expected: {expectedValue:o}");
        Assert.Equal(expectedValue, result.Value);

        // BSON serialization
        using var ms = new MemoryStream();
        using (var writer = new Newtonsoft.Json.Bson.BsonDataWriter(ms))
        {
            var serializer = new Newtonsoft.Json.JsonSerializer();
            serializer.Serialize(writer, result);
        }

        var bsonBytes = ms.ToArray();

        // BSON deserialization
        using var ms2 = new MemoryStream(bsonBytes);
        using (var reader = new Newtonsoft.Json.Bson.BsonDataReader(ms2))
        {
            var serializer = new Newtonsoft.Json.JsonSerializer();
            var bsonDeserialized = serializer.Deserialize<DateTimeOffsetPrimowrapClassWithPredefinedProperty>(reader);

            testOutputHelper.WriteLine($"Deserialized Actual: {bsonDeserialized?.Value:o}");

            // *** FIX FOR THE TEST ***
            // BSON loses sub-millisecond precision. We must truncate the expected value
            // to match the precision of the actual value before comparing.

            // 1. Get the Ticks of the original UTC DateTime
            long originalTicks = expectedValue.UtcTicks;

            // 2. Truncate the ticks to the nearest millisecond
            long truncatedTicks = originalTicks - (originalTicks % TimeSpan.TicksPerMillisecond);

            // 3. Create a new DateTimeOffset with the truncated value for comparison
            var expectedValueTruncated = new DateTimeOffset(truncatedTicks, TimeSpan.Zero);

            testOutputHelper.WriteLine($"Truncated Expected:  {expectedValueTruncated:o}");

            Assert.Equal(expectedValueTruncated, bsonDeserialized?.Value);
        }
    }

    [Fact]
    public void DateTimeOffsetWrapperClassWithPredefinedProperty_IgnoresReadonly_WhenSerializedNewtonsoftBson()
    {
        var expectedValue = DateTimeOffset.MinValue;
        var result = DateTimeOffsetPrimowrapClassWithPredefinedProperty.From(expectedValue);

        // Default value to string
        testOutputHelper.WriteLine("result.Value:");
        testOutputHelper.WriteLine(result.Value.ToString());

        Assert.Equal(expectedValue, result.Value);

        // BSON serialization
        using var ms = new MemoryStream();
        using (var writer = new Newtonsoft.Json.Bson.BsonDataWriter(ms))
        {
            var serializer = new Newtonsoft.Json.JsonSerializer();
            serializer.Serialize(writer, result);
        }

        var bsonBytes = ms.ToArray();
        var bsonBase64 = Convert.ToBase64String(bsonBytes);
        testOutputHelper.WriteLine("\nBSON serialization (Base64):");
        testOutputHelper.WriteLine(bsonBase64);

        // BSON deserialization
        using var ms2 = new MemoryStream(bsonBytes);
        using (var reader = new Newtonsoft.Json.Bson.BsonDataReader(ms2))
        {
            var serializer = new Newtonsoft.Json.JsonSerializer();
            var bsonDeserialized = serializer.Deserialize<DateTimeOffsetPrimowrapClassWithPredefinedProperty>(reader);
            testOutputHelper.WriteLine("\nBSON deserialized value:");
            testOutputHelper.WriteLine(bsonDeserialized?.ToString() ?? "null");

            // Compare the underlying DateTime values directly to avoid timezone/offset issues
            var expectedDateTime = expectedValue.UtcDateTime;
            var actualDateTime = bsonDeserialized?.Value.UtcDateTime;
            Assert.Equal(expectedDateTime, actualDateTime);
        }
    }

    [Fact]
    public void DateTimeOffsetWrapperClassWithPredefinedProperty_IgnoresReadonly_WhenSerializedNewtonsoftBsonV2()
    {
        var expectedValue = DateTimeOffset.MinValue;
        var result = DateTimeOffsetPrimowrapClassWithPredefinedProperty.From(expectedValue);

        // Default value to string
        testOutputHelper.WriteLine("result.Value:");
        testOutputHelper.WriteLine(result.Value.ToString("o")); // Use ISO 8601 for clarity

        Assert.Equal(expectedValue, result.Value);

        // **SOLUTION: Configure the serializer to handle dates as UTC.**
        var serializerSettings = new Newtonsoft.Json.JsonSerializerSettings
        {
            DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc
        };
        var serializer = Newtonsoft.Json.JsonSerializer.Create(serializerSettings);

        // BSON serialization
        using var ms = new MemoryStream();
        using (var writer = new Newtonsoft.Json.Bson.BsonDataWriter(ms))
        {
            serializer.Serialize(writer, result);
        }

        var bsonBytes = ms.ToArray();
        var bsonBase64 = Convert.ToBase64String(bsonBytes);
        testOutputHelper.WriteLine("\nBSON serialization (Base64):");
        testOutputHelper.WriteLine(bsonBase64);

        // BSON deserialization
        using var ms2 = new MemoryStream(bsonBytes);
        using (var reader = new Newtonsoft.Json.Bson.BsonDataReader(ms2))
        {
            // Ensure the reader also respects the UTC setting
            reader.DateTimeKindHandling = System.DateTimeKind.Utc;

            var bsonDeserialized = serializer.Deserialize<DateTimeOffsetPrimowrapClassWithPredefinedProperty>(reader);
            testOutputHelper.WriteLine("\nBSON deserialized value:");
            testOutputHelper.WriteLine(bsonDeserialized?.Value.ToString("o") ?? "null");

            // The assertion should now pass without any special handling.
            Assert.Equal(expectedValue, bsonDeserialized?.Value);
        }
    }
}

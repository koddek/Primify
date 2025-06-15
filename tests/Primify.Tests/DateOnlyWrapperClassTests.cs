namespace Primify.Generator.Tests;

[Primify<DateOnly>]
public partial class DateOnlyPrimowrapClass;

// [Primify<DateOnly>]
// public partial class DateOnlyWrapperClassWithNormalize
// {
//     private static DateOnly Normalize(DateOnly value) => value < 1 ? -1 : value;
// }

[Primify<DateOnly>]
public partial class DateOnlyPrimowrapClassWithPredefinedProperty
{
    public static DateOnlyPrimowrapClassWithPredefinedProperty Empty => new(DateOnly.MinValue);
}

public class DateOnlyWrapperClassTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void DateOnlyWrapperClass_CreatesType_WhenFromIsCalled()
    {
        // Arrange
        var expectedValue = DateOnly.MinValue;

        // Act
        var result = DateOnlyPrimowrapClass.From(expectedValue);
        testOutputHelper.WriteLine(result.ToString());

        // Assert
        Assert.Equal(expectedValue, result.Value);
        Assert.Equal(expectedValue, result.Value);
    }

    [Fact]
    public void DateOnlyWrapperClass_CreatesType_WhenExplicitlyCastToFromType()
    {
        var expectedValue = DateOnly.MinValue;

        var result1 = (DateOnlyPrimowrapClass)expectedValue;
        var result2 = (DateOnly)result1;
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
    // public void DateOnlyWrapperClassWithNormalize_ReturnsNormalizedValue_WhenCalledWithNonNormalizedValue(int value, int expected)
    // {
    //     var result = DateOnlyWrapperClassWithNormalize.From(value);
    //     testOutputHelper.WriteLine(result.ToString());
    //
    //     Assert.Equal(expected, result.Value);
    // }

    [Fact]
    public void DateOnlyWrapperClassWithPredefinedProperty_IgnoresReadonly_WhenSerializedWithSystemTextJsonV1()
    {
        var expectedValue = DateOnly.FromDateTime(DateTime.Now);
        var result = DateOnlyPrimowrapClassWithPredefinedProperty.From(expectedValue);

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
            System.Text.Json.JsonSerializer.Deserialize<DateOnlyPrimowrapClassWithPredefinedProperty>(json);
        testOutputHelper.WriteLine("\nSystem.Text.Json deserialized value:");
        testOutputHelper.WriteLine(stjDeserialized?.ToString() ?? "null");
        Assert.Equal(expectedValue, stjDeserialized?.Value);
    }

    [Fact]
    public void DateOnlyWrapperClassWithPredefinedProperty_IgnoresReadonly_WhenSerializedWithSystemTextJson()
    {
        var expectedValue = DateOnly.MinValue;
        var result = DateOnlyPrimowrapClassWithPredefinedProperty.Empty;

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
            System.Text.Json.JsonSerializer.Deserialize<DateOnlyPrimowrapClassWithPredefinedProperty>(json);
        testOutputHelper.WriteLine("\nSystem.Text.Json deserialized value:");
        testOutputHelper.WriteLine(stjDeserialized?.ToString() ?? "null");
        Assert.Equal(expectedValue, stjDeserialized?.Value);
    }

    [Fact]
    public void DateOnlyWrapperClassWithPredefinedProperty_IgnoresReadonly_WhenSerializedNewtonsoftJsonV1()
    {
        var expectedValue = DateOnly.FromDateTime(DateTime.Now);
        var result = DateOnlyPrimowrapClassWithPredefinedProperty.From(expectedValue);

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
            Newtonsoft.Json.JsonConvert.DeserializeObject<DateOnlyPrimowrapClassWithPredefinedProperty>(newtonsoftJson);
        testOutputHelper.WriteLine("\nNewtonsoft.Json deserialized value:");
        testOutputHelper.WriteLine(njsDeserialized?.ToString() ?? "null");
        Assert.Equal(expectedValue, njsDeserialized?.Value);
    }

    [Fact]
    public void DateOnlyWrapperClassWithPredefinedProperty_IgnoresReadonly_WhenSerializedNewtonsoftJson()
    {
        var expectedValue = DateOnly.MinValue;
        var result = DateOnlyPrimowrapClassWithPredefinedProperty.Empty;

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
            Newtonsoft.Json.JsonConvert.DeserializeObject<DateOnlyPrimowrapClassWithPredefinedProperty>(newtonsoftJson);
        testOutputHelper.WriteLine("\nNewtonsoft.Json deserialized value:");
        testOutputHelper.WriteLine(njsDeserialized?.ToString() ?? "null");
        Assert.Equal(expectedValue, njsDeserialized?.Value);
    }

    [Fact]
    public void DateOnlyWrapperClassWithPredefinedProperty_IgnoresReadonly_WhenSerializedNewtonsoftBsonV1()
    {
        var expectedValue = DateOnly.FromDateTime(DateTime.Now);
        var result = DateOnlyPrimowrapClassWithPredefinedProperty.From(expectedValue);

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
            var bsonDeserialized = serializer.Deserialize<DateOnlyPrimowrapClassWithPredefinedProperty>(reader);
            testOutputHelper.WriteLine("\nBSON deserialized value:");
            testOutputHelper.WriteLine(bsonDeserialized?.ToString() ?? "null");
            Assert.Equal(expectedValue, bsonDeserialized?.Value);
        }
    }

    [Fact]
    public void DateOnlyWrapperClassWithPredefinedProperty_IgnoresReadonly_WhenSerializedNewtonsoftBson()
    {
        var expectedValue = DateOnly.MinValue;
        var result = DateOnlyPrimowrapClassWithPredefinedProperty.Empty;

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
            var bsonDeserialized = serializer.Deserialize<DateOnlyPrimowrapClassWithPredefinedProperty>(reader);
            testOutputHelper.WriteLine("\nBSON deserialized value:");
            testOutputHelper.WriteLine(bsonDeserialized?.ToString() ?? "null");
            Assert.Equal(expectedValue, bsonDeserialized?.Value);
        }
    }
}

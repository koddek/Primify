namespace Primify.Generator.Tests;

[Primify<Guid>]
public partial class GuidPrimowrapClass;

// [Primify<Guid>]
// public partial class GuidWrapperClassWithNormalize
// {
//     private static Guid Normalize(Guid value) => value < 1 ? -1 : value;
// }

[Primify<Guid>]
public partial class GuidPrimowrapClassWithPredefinedProperty
{
    public static GuidPrimowrapClassWithPredefinedProperty Empty => new(Guid.Empty);
}

public class GuidWrapperClassTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void GuidWrapperClass_CreatesType_WhenFromIsCalled()
    {
        // Arrange
        var expectedValue = Guid.Empty;

        // Act
        var result = GuidPrimowrapClass.From(expectedValue);
        testOutputHelper.WriteLine(result.ToString());

        // Assert
        Assert.Equal(expectedValue, result.Value);
        Assert.Equal(expectedValue, result.Value);
    }

    [Fact]
    public void GuidWrapperClass_CreatesType_WhenExplicitlyCastToFromType()
    {
        var expectedValue = Guid.Empty;

        var result1 = (GuidPrimowrapClass)expectedValue;
        var result2 = (Guid)result1;
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
    // public void GuidWrapperClassWithNormalize_ReturnsNormalizedValue_WhenCalledWithNonNormalizedValue(int value, int expected)
    // {
    //     var result = GuidWrapperClassWithNormalize.From(value);
    //     testOutputHelper.WriteLine(result.ToString());
    //
    //     Assert.Equal(expected, result.Value);
    // }

    [Fact]
    public void GuidWrapperClassWithPredefinedProperty_IgnoresReadonly_WhenSerializedWithSystemTextJsonV1()
    {
        var expectedValue = Guid.NewGuid();
        var result = GuidPrimowrapClassWithPredefinedProperty.From(expectedValue);

        // Default value to string
        testOutputHelper.WriteLine("result.Value:");
        testOutputHelper.WriteLine(result.Value.ToString());

        Assert.Equal(expectedValue, result.Value);

        // System.Text.Json serialization
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        testOutputHelper.WriteLine("\nSystem.Text.Json serialization:");
        testOutputHelper.WriteLine(json);

        // System.Text.Json deserialization
        var stjDeserialized = System.Text.Json.JsonSerializer.Deserialize<GuidPrimowrapClassWithPredefinedProperty>(json);
        testOutputHelper.WriteLine("\nSystem.Text.Json deserialized value:");
        testOutputHelper.WriteLine(stjDeserialized?.ToString() ?? "null");
        Assert.Equal(expectedValue, stjDeserialized?.Value);
    }

    [Fact]
    public void GuidWrapperClassWithPredefinedProperty_IgnoresReadonly_WhenSerializedWithSystemTextJson()
    {
        var expectedValue = Guid.Empty;
        var result = GuidPrimowrapClassWithPredefinedProperty.Empty;

        // Default value to string
        testOutputHelper.WriteLine("result.Value:");
        testOutputHelper.WriteLine(result.Value.ToString());

        Assert.Equal(expectedValue, result.Value);

        // System.Text.Json serialization
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        testOutputHelper.WriteLine("\nSystem.Text.Json serialization:");
        testOutputHelper.WriteLine(json);

        // System.Text.Json deserialization
        var stjDeserialized = System.Text.Json.JsonSerializer.Deserialize<GuidPrimowrapClassWithPredefinedProperty>(json);
        testOutputHelper.WriteLine("\nSystem.Text.Json deserialized value:");
        testOutputHelper.WriteLine(stjDeserialized?.ToString() ?? "null");
        Assert.Equal(expectedValue, stjDeserialized?.Value);
    }

    [Fact]
    public void GuidWrapperClassWithPredefinedProperty_IgnoresReadonly_WhenSerializedNewtonsoftJsonV1()
    {
        var expectedValue = Guid.NewGuid();
        var result = GuidPrimowrapClassWithPredefinedProperty.From(expectedValue);

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
            Newtonsoft.Json.JsonConvert.DeserializeObject<GuidPrimowrapClassWithPredefinedProperty>(newtonsoftJson);
        testOutputHelper.WriteLine("\nNewtonsoft.Json deserialized value:");
        testOutputHelper.WriteLine(njsDeserialized?.ToString() ?? "null");
        Assert.Equal(expectedValue, njsDeserialized?.Value);
    }

    [Fact]
    public void GuidWrapperClassWithPredefinedProperty_IgnoresReadonly_WhenSerializedNewtonsoftJson()
    {
        var expectedValue = Guid.Empty;
        var result = GuidPrimowrapClassWithPredefinedProperty.Empty;

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
            Newtonsoft.Json.JsonConvert.DeserializeObject<GuidPrimowrapClassWithPredefinedProperty>(newtonsoftJson);
        testOutputHelper.WriteLine("\nNewtonsoft.Json deserialized value:");
        testOutputHelper.WriteLine(njsDeserialized?.ToString() ?? "null");
        Assert.Equal(expectedValue, njsDeserialized?.Value);
    }

    [Fact]
    public void GuidWrapperClassWithPredefinedProperty_IgnoresReadonly_WhenSerializedNewtonsoftBsonV1()
    {
        var expectedValue = Guid.NewGuid();
        var result = GuidPrimowrapClassWithPredefinedProperty.From(expectedValue);

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
            var bsonDeserialized = serializer.Deserialize<GuidPrimowrapClassWithPredefinedProperty>(reader);
            testOutputHelper.WriteLine("\nBSON deserialized value:");
            testOutputHelper.WriteLine(bsonDeserialized?.ToString() ?? "null");
            Assert.Equal(expectedValue, bsonDeserialized?.Value);
        }
    }

    [Fact]
    public void GuidWrapperClassWithPredefinedProperty_IgnoresReadonly_WhenSerializedNewtonsoftBson()
    {
        var expectedValue = Guid.Empty;
        var result = GuidPrimowrapClassWithPredefinedProperty.Empty;

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
            var bsonDeserialized = serializer.Deserialize<GuidPrimowrapClassWithPredefinedProperty>(reader);
            testOutputHelper.WriteLine("\nBSON deserialized value:");
            testOutputHelper.WriteLine(bsonDeserialized?.ToString() ?? "null");
            Assert.Equal(expectedValue, bsonDeserialized?.Value);
        }
    }
}

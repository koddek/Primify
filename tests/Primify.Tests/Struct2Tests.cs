namespace Primify.Generator.Tests;

[Primify<string>]
public partial struct Struct2;

[Primify<string>]
public partial struct Struct2WithNormalize
{
    private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? "" : value;
}

[Primify<string>]
public partial struct Struct2WithPredefinedProperty
{
    public static Struct2WithPredefinedProperty Empty => new("");
    public static Struct2WithPredefinedProperty Undefined => new("UNDEFINED");
}

public class Struct2Tests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Struct2_CreatesType_WhenFromIsCalled()
    {
        // Arrange
        var expectedValue = "foo";

        // Act
        var result = Struct2.From(expectedValue);
        testOutputHelper.WriteLine(result.ToString());

        // Assert
        Assert.Equal(expectedValue, result.Value);
        Assert.Equal(expectedValue, result.Value);
    }

    [Fact]
    public void Struct2_CreatesType_WhenExplicitlyCastToFromType()
    {
        var expectedValue = "foo";

        var result1 = (Struct2)expectedValue;
        var result2 = (string)result1;
        testOutputHelper.WriteLine(result1.ToString());

        Assert.Equal(expectedValue, result1.Value);
        Assert.Equal(expectedValue, result1.Value);
        Assert.Equal(expectedValue, result2);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(null, "")]
    [InlineData(" ", "")]
    public void Struct2WithNormalize_ReturnsNormalizedValue_WhenCalledWithNonNormalizedValue(string value,
        string expected)
    {
        var result = Struct2WithNormalize.From(value);
        testOutputHelper.WriteLine(result.ToString());

        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void Struct2WithPredefinedProperty_IgnoresReadonly_WhenSerializedWithSystemTextJson()
    {
        var expectedValue = "";
        var result = Struct2WithPredefinedProperty.Empty;

        // Default value to string
        testOutputHelper.WriteLine("result.Value:");
        testOutputHelper.WriteLine(result.Value);

        Assert.Equal(expectedValue, result.Value);

        // System.Text.Json serialization
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        testOutputHelper.WriteLine("\nSystem.Text.Json serialization:");
        testOutputHelper.WriteLine(json);

        // System.Text.Json deserialization
        var stjDeserialized = System.Text.Json.JsonSerializer.Deserialize<Struct2WithPredefinedProperty>(json);
        testOutputHelper.WriteLine("\nSystem.Text.Json deserialized value:");
        testOutputHelper.WriteLine(stjDeserialized.ToString() ?? "null");
        Assert.Equal(expectedValue, stjDeserialized.Value);
    }

    [Fact]
    public void Struct2WithPredefinedProperty_IgnoresReadonly_WhenSerializedNewtonsoftJson()
    {
        var expectedValue = "";
        var result = Struct2WithPredefinedProperty.Empty;

        // Default value to string
        testOutputHelper.WriteLine("result.Value:");
        testOutputHelper.WriteLine(result.Value);

        Assert.Equal(expectedValue, result.Value);

        // Newtonsoft.Json serialization
        var newtonsoftJson = Newtonsoft.Json.JsonConvert.SerializeObject(result);
        testOutputHelper.WriteLine("\nNewtonsoft.Json serialization:");
        testOutputHelper.WriteLine(newtonsoftJson);

        // Newtonsoft.Json deserialization
        var njsDeserialized =
            Newtonsoft.Json.JsonConvert.DeserializeObject<Struct2WithPredefinedProperty>(newtonsoftJson);
        testOutputHelper.WriteLine("\nNewtonsoft.Json deserialized value:");
        testOutputHelper.WriteLine(njsDeserialized.ToString() ?? "null");
        Assert.Equal(expectedValue, njsDeserialized.Value);
    }

    [Fact]
    public void Struct2WithPredefinedProperty_IgnoresReadonly_WhenSerializedNewtonsoftBson()
    {
        var expectedValue = "";
        var result = Struct2WithPredefinedProperty.Empty;

        // Default value to string
        testOutputHelper.WriteLine("result.Value:");
        testOutputHelper.WriteLine(result.Value);

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
            var bsonDeserialized = serializer.Deserialize<Struct2WithPredefinedProperty>(reader);
            testOutputHelper.WriteLine("\nBSON deserialized value:");
            testOutputHelper.WriteLine(bsonDeserialized.ToString() ?? "null");
            Assert.Equal(expectedValue, bsonDeserialized.Value);
        }
    }
}

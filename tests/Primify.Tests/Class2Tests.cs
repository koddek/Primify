namespace Primify.Generator.Tests;

[Primify<string>]
public partial class Class2;

[Primify<string>]
public partial class Class2WithNormalize
{
    private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? "" : value;
}

[Primify<string>]
public partial class Class2WithPredefinedProperty
{
    public static Class2WithPredefinedProperty Empty => new("");
    public static Class2WithPredefinedProperty Undefined => new("UNDEFINED");
}

public class Class2Tests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Class2_CreatesType_WhenFromIsCalled()
    {
        // Arrange
        var expectedValue = "foo";

        // Act
        var result = Class2.From(expectedValue);
        testOutputHelper.WriteLine(result.ToString());

        // Assert
        Assert.Equal(expectedValue, result.Value);
        Assert.Equal(expectedValue, result.Value);
    }

    [Fact]
    public void Class2_CreatesType_WhenExplicitlyCastToFromType()
    {
        var expectedValue = "foo";

        var result1 = (Class2)expectedValue;
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
    public void Class2WithNormalize_ReturnsNormalizedValue_WhenCalledWithNonNormalizedValue(string value,
        string expected)
    {
        var result = Class2WithNormalize.From(value);
        testOutputHelper.WriteLine(result.ToString());

        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void Class2WithPredefinedProperty_IgnoresReadonly_WhenSerializedWithSystemTextJson()
    {
        var expectedValue = "UNDEFINED";
        var result = Class2WithPredefinedProperty.Undefined;

        // Default value to string
        testOutputHelper.WriteLine("result.Value:");
        testOutputHelper.WriteLine(result.Value);

        Assert.Equal(expectedValue, result.Value);

        // System.Text.Json serialization
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        testOutputHelper.WriteLine("\nSystem.Text.Json serialization:");
        testOutputHelper.WriteLine(json);

        // System.Text.Json deserialization
        var stjDeserialized = System.Text.Json.JsonSerializer.Deserialize<Class2WithPredefinedProperty>(json);
        testOutputHelper.WriteLine("\nSystem.Text.Json deserialized value:");
        testOutputHelper.WriteLine(stjDeserialized?.ToString() ?? "null");
        Assert.Equal(expectedValue, stjDeserialized?.Value);
    }

    [Fact]
    public void Class2WithPredefinedProperty_IgnoresReadonly_WhenSerializedNewtonsoftJson()
    {
        var expectedValue = "UNDEFINED";
        var result = Class2WithPredefinedProperty.Undefined;

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
            Newtonsoft.Json.JsonConvert.DeserializeObject<Class2WithPredefinedProperty>(newtonsoftJson);
        testOutputHelper.WriteLine("\nNewtonsoft.Json deserialized value:");
        testOutputHelper.WriteLine(njsDeserialized?.ToString() ?? "null");
        Assert.Equal(expectedValue, njsDeserialized?.Value);
    }

    [Fact]
    public void Class2WithPredefinedProperty_IgnoresReadonly_WhenSerializedNewtonsoftBson()
    {
        var expectedValue = "UNDEFINED";
        var result = Class2WithPredefinedProperty.Undefined;

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
        testOutputHelper.WriteLine(bsonBytes.ToString());
        var bsonBase64 = Convert.ToBase64String(bsonBytes);
        testOutputHelper.WriteLine("\nBSON serialization (Base64):");
        testOutputHelper.WriteLine(bsonBase64);

        // BSON deserialization
        using var ms2 = new MemoryStream(bsonBytes);
        using (var reader = new Newtonsoft.Json.Bson.BsonDataReader(ms2))
        {
            var serializer = new Newtonsoft.Json.JsonSerializer();
            var bsonDeserialized = serializer.Deserialize<Class2WithPredefinedProperty>(reader);
            testOutputHelper.WriteLine("\nBSON deserialized value:");
            testOutputHelper.WriteLine(bsonDeserialized?.ToString() ?? "null");
            Assert.Equal(expectedValue, bsonDeserialized?.Value);
        }
    }
}

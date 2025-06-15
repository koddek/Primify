using System.Text.Json;
using Primify.Attributes;
using Primify.Converters;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Primify.Generator.Tests;

[Primify<int>]
public partial class Class1;

[Primify<int>]
public partial class Class1WithNormalize
{
    private static int Normalize(int value) => value < 1 ? -1 : value;
}

[Primify<int>]
public partial class Class1WithPredefinedProperty
{
    public static Class1WithPredefinedProperty Empty => new(-1);
}

public class Class1Tests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Class1_CreatesType_WhenFromIsCalled()
    {
        // Arrange
        int expectedValue = 1001;

        // Act
        var result = Class1.From(expectedValue);
        testOutputHelper.WriteLine(result.ToString());

        // Assert
        Assert.Equal(expectedValue, result.Value);
        Assert.Equal(expectedValue, result.Value);
    }

    [Fact]
    public void Class1_CreatesType_WhenExplicitlyCastToFromType()
    {
        int expectedValue = 1001;

        Class1 result1 = (Class1)expectedValue;
        int result2 = (int)result1;
        testOutputHelper.WriteLine(result1.ToString());

        Assert.Equal(expectedValue, result1.Value);
        Assert.Equal(expectedValue, result1.Value);
        Assert.Equal(expectedValue, result2);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(10, 10)]
    [InlineData(0, -1)]
    [InlineData(-1, -1)]
    [InlineData(-100, -1)]
    public void Class1WithNormalize_ReturnsNormalizedValue_WhenCalledWithNonNormalizedValue(int value, int expected)
    {
        var result = Class1WithNormalize.From(value);
        testOutputHelper.WriteLine(result.ToString());

        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void Class1WithPredefinedProperty_IgnoresReadonly_WhenSerializedWithSystemTextJson()
    {
        var expectedValue = -1;
        var result = Class1WithPredefinedProperty.Empty;

        // Default value to string
        testOutputHelper.WriteLine("result.Value:");
        testOutputHelper.WriteLine(result.Value.ToString());

        Assert.Equal(expectedValue, result.Value);

         // System.Text.Json serialization
         var json = JsonSerializer.Serialize(result);
         testOutputHelper.WriteLine("\nSystem.Text.Json serialization:");
         testOutputHelper.WriteLine(json);

         // System.Text.Json deserialization
         var stjDeserialized = JsonSerializer.Deserialize<Class1WithPredefinedProperty>(json);
         testOutputHelper.WriteLine("\nSystem.Text.Json deserialized value:");
         testOutputHelper.WriteLine(stjDeserialized?.ToString() ?? "null");
         Assert.Equal(expectedValue, stjDeserialized?.Value);
    }

    [Fact]
    public void Class1WithPredefinedProperty_IgnoresReadonly_WhenSerializedNewtonsoftJson()
    {
        var expectedValue = -1;
        var result = Class1WithPredefinedProperty.Empty;

        // Default value to string
        testOutputHelper.WriteLine("result.Value:");
        testOutputHelper.WriteLine(result.Value.ToString());

        Assert.Equal(expectedValue, result.Value);

        // Newtonsoft.Json serialization
        var newtonsoftJson = Newtonsoft.Json.JsonConvert.SerializeObject(result);
        testOutputHelper.WriteLine("\nNewtonsoft.Json serialization:");
        testOutputHelper.WriteLine(newtonsoftJson);

        // Newtonsoft.Json deserialization
        var njsDeserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<Class1WithPredefinedProperty>(newtonsoftJson);
        testOutputHelper.WriteLine("\nNewtonsoft.Json deserialized value:");
        testOutputHelper.WriteLine(njsDeserialized?.ToString() ?? "null");
        Assert.Equal(expectedValue, njsDeserialized?.Value);
    }

    [Fact]
    public void Class1WithPredefinedProperty_IgnoresReadonly_WhenSerializedNewtonsoftBson()
    {
        var expectedValue = -1;
        var result = Class1WithPredefinedProperty.Empty;

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
             var bsonDeserialized = serializer.Deserialize<Class1WithPredefinedProperty>(reader);
             testOutputHelper.WriteLine("\nBSON deserialized value:");
             testOutputHelper.WriteLine(bsonDeserialized?.ToString() ?? "null");
             Assert.Equal(expectedValue, bsonDeserialized?.Value);
         }
    }
}

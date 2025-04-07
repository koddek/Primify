using System;
using Newtonsoft.Json;
using System.Text.Json;
using System.Threading.Tasks;
using LiteDB;
using Primify.Tests.Models;
using TUnit;
using TUnit.Assertions.AssertConditions.Throws;
using JsonSerializer = LiteDB.JsonSerializer;

namespace Primify.Tests;

public class BasicUsageTests
{
    [Test]
    public async Task OrderId_CreationAndImplicitConversion_Works()
    {
        // Arrange
        int expectedValue = 1001;
        var orderId = OrderId.From(expectedValue);

        // Act
        int actualValue = (int)orderId; // Implicit conversion to int
        OrderId fromPrimitive = expectedValue; // Implicit conversion from int

        // Assert
        await Assert.That(actualValue).IsEqualTo(expectedValue);
        await Assert.That(fromPrimitive).IsEqualTo(orderId);
    }

    [Test]
    public async Task CustomerName_CreationAndImplicitConversion_Works()
    {
        // Arrange
        string expectedValue = "Alice Smith";
        var customerName = CustomerName.From(expectedValue);

        // Act
        string actualValue = (string)customerName; // Explicit conversion to string
        CustomerName fromPrimitive = expectedValue; // Implicit conversion from string

        // Assert
        await Assert.That(actualValue).IsEqualTo(expectedValue);
        await Assert.That(fromPrimitive).IsEqualTo(customerName);
    }

    // [Test]
    // public async Task Order_SerializesWithSystemTextJson_Correctly()
    // {
    //     // Arrange
    //     var expectedValue = "{\"Id\":1001,\"Customer\":\"Alice Smith\",\"Quantity\":5}";
    //     var order = new Order
    //     {
    //         Id = OrderId.From(1001),
    //         Customer = CustomerName.From("Alice Smith"),
    //         Quantity = Quantity.From(5)
    //     };
    //
    //     // Act
    //     string json = JsonSerializer.Serialize(order);
    //     var deserialized = JsonSerializer.Deserialize<Order>(json);
    //
    //     // Assert
    //     await Assert.That(json).IsEqualTo(expectedValue);
    //     await Assert.That(deserialized).IsEquivalentTo(order);
    // }

    [Test]
    public async Task Order_SerializesWithNewtonsoftJson_Correctly()
    {
        // Arrange
        var expectedValue = "{\"Id\":1001,\"Customer\":\"Alice Smith\",\"Quantity\":5}";
        var order = new Order
        {
            Id = OrderId.From(1001),
            Customer = CustomerName.From("Alice Smith"),
            Quantity = Quantity.From(5)
        };

        // Act
        string json = JsonConvert.SerializeObject(order);
        var deserialized = JsonConvert.DeserializeObject<Order>(json);

        // Assert
        await Assert.That(json).IsEqualTo(expectedValue);
        await Assert.That(deserialized).IsEquivalentTo(order);
    }

    [Test]
    public async Task Order_PersistsToLiteDB_Correctly()
    {
        // Arrange
        var order = new Order
        {
            Id = OrderId.From(1001),
            Customer = CustomerName.From("Alice Smith"),
            Quantity = Quantity.From(5)
        };

        // Act
        using var db = new LiteDatabase(":memory:");
        var col = db.GetCollection<Order>("orders");
        col.Insert(order);
        var retrieved = col.FindOne(o => o.Id == OrderId.From(1001));

        // Assert
        await Assert.That(retrieved).IsEquivalentTo(order);
    }

    [Test]
    public async Task EmailAddress_ReturnsEmpty_WhenCreatedEmpty()
    {
        // Arrange
        var expected = string.Empty;

        // Act
        var email = EmailAddress.Empty;

        // Assert
        await Assert.That(email.Value).IsEqualTo(expected);
    }

    [Test]
    public async Task EmailAddress_ReturnsUndefined_WhenCreatedUndefined()
    {
        // Arrange
        var expected = "undefined@example.com";

        // Act
        var email = EmailAddress.Undefined;

        // Assert
        await Assert.That(email.Value).IsEqualTo(expected);
    }

    [Test]
    public async Task Number_ReturnsEmpty_WhenCreatedEmpty()
    {
        // Arrange
        var expected = 0;

        // Act
        var result = Number.Empty;

        // Assert
        await Assert.That(result.Value).IsEqualTo(expected);
    }

    [Test]
    public async Task Number_ReturnsUndefined_WhenCreatedUndefined()
    {
        // Arrange
        var expected = -1;

        // Act
        var result = Number.Undefined;

        // Assert
        await Assert.That(result.Value).IsEqualTo(expected);
    }

    [Test]
    public async Task ItemId_ReturnsEmpty_WhenCreatedEmpty()
    {
        // Arrange
        var expected = Guid.Empty.ToString();

        // Act
        var result = ItemId.Empty;
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        Console.WriteLine(json);

        // Assert
        await Assert.That(result.Value.ToString()).IsEqualTo(expected);
    }

    [Test]
    public async Task ItemId_ReturnsUndefined_WhenCreatedUndefined()
    {
        // Arrange
        var expected = "ffffffff-ffff-ffff-ffff-ffffffffffff";

        //Act
        var result = ItemId.Undefined;

        // Assert
        await Assert.That(result.ToString()).IsEqualTo(expected);
    }

    [Test]
    public async Task Username_ReturnsUndefinedValue_Correctly()
    {
        // Arrange
        var expected = "guest";

        // Act
        var username = Username.Undefined;

        // Assert
        await Assert.That(username.Value).IsEquivalentTo(expected);
    }

    [Test]
    public async Task PredefinedValues_AreCorrectlyInitialized()
    {
        // Test both predefined values
        await Assert.That(Username.Empty.Value).IsEquivalentTo("");
        await Assert.That(Username.Undefined.Value).IsEquivalentTo("guest");
    }

    [Test]
    public async Task FromMethod_CreatesInstanceWithCorrectValue()
    {
        // Arrange
        var testValue = "testUser";

        // Act
        var username = Username.From(testValue);

        // Assert
        await Assert.That(username.Value).IsEquivalentTo(testValue.ToLower()); // Tests normalization
    }

    [Test]
    public async Task ImplicitConversion_WorksCorrectly()
    {
        // Arrange
        var testValue = "implicitUser";

        // Act
        Username username = testValue; // Uses implicit conversion

        // Assert
        await Assert.That(username.Value).IsEquivalentTo(testValue.ToLower());
    }

    [Test]
    public async Task ExplicitConversion_WorksCorrectly()
    {
        // Arrange
        var username = Username.From("explicitUser");

        // Act
        var stringValue = (string)username; // Uses explicit conversion

        // Assert
        await Assert.That(stringValue).IsEquivalentTo("explicituser");
    }

    [Test]
    public async Task Validation_RejectsInvalidValues()
    {
        // Arrange
        var invalidValue = "invalid@user";

        // Act & Assert
        await Assert.That(() => Username.From(invalidValue))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task Normalization_AppliesCorrectly()
    {
        // Arrange
        var mixedCase = "MixedCaseUser";

        // Act
        var username = Username.From(mixedCase);

        // Assert
        await Assert.That(username.Value).IsEquivalentTo(mixedCase.ToLower());
    }

    [Test]
    public async Task JsonSerialization_WorksCorrectly()
    {
        // Arrange
        var username = Username.From("jsonUser");
        var expectedJson = $"\"{username.Value}\"";

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(username);

        // Assert
        await Assert.That(json).IsEquivalentTo(expectedJson);
    }

    [Test]
    public async Task JsonDeserialization_WorksCorrectly()
    {
        // Arrange
        var testValue = "deserializedUser";
        var json = $"\"{testValue}\"";

        // Act
        var username = System.Text.Json.JsonSerializer.Deserialize<Username>(json);

        // Assert
        await Assert.That(username.Value).IsEquivalentTo(testValue.ToLower());
    }

    [Test]
    public async Task ToString_ReturnsUnderlyingValue()
    {
        // Arrange
        var testValue = "stringUser";
        var username = Username.From(testValue);

        // Act & Assert
        await Assert.That(username.ToString()).IsEquivalentTo(testValue.ToLower());
    }

    [Test]
    public async Task PredefinedValues_AreIgnoredInSerialization()
    {
        // Arrange
        var expected = """{"Username":"guest"}""";
        var obj = new { Username = Username.Undefined };

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(obj);
        Console.WriteLine(json);

        // Assert
        await Assert.That(json).IsEquivalentTo(expected);
    }
}

using System;
using Newtonsoft.Json;
using System.Text.Json;
using System.Threading.Tasks;
using LiteDB;
using Primify.Tests.Models;
using TUnit;
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
        string actualValue = (string)customerName; // Implicit conversion to string
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

    /*[Test]
    public async Task ItemId_PersistsToLiteDB_Correctly()
    {
        // Arrange
        var order = new Order
        {
            Id = OrderId.From(),
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
    }*/
}

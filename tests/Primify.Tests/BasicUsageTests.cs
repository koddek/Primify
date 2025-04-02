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
        int actualValue = orderId; // Implicit conversion to int
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
        string actualValue = customerName; // Implicit conversion to string
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
}

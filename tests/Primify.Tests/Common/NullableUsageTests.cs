namespace Primify.Generator.Tests.Common;

using System.Text.Json;

public class NullableUsageTests
{
    private sealed class Order
    {
        public int Id { get; set; }

        public IntRecordStruct? Quantity { get; set; }
    }

    [Test]
    public async Task SystemTextJson_Preserves_Null_And_Value()
    {
        var withValue = new Order { Id = 1, Quantity = IntRecordStruct.From(3) };
        var withoutValue = new Order { Id = 2 };

        var valueJson = JsonSerializer.Serialize(withValue);
        var nullJson = JsonSerializer.Serialize(withoutValue);

        var valueBack = JsonSerializer.Deserialize<Order>(valueJson);
        var nullBack = JsonSerializer.Deserialize<Order>(nullJson);

        await Assert.That(valueBack!.Quantity).IsEqualTo(IntRecordStruct.From(3));
        await Assert.That(nullBack!.Quantity).IsNull();
    }

    [Test]
    public async Task NewtonsoftJson_Preserves_Null_And_Value()
    {
        var withValue = new Order { Id = 1, Quantity = IntRecordStruct.From(3) };
        var withoutValue = new Order { Id = 2 };

        var valueBack = Newtonsoft.Json.JsonConvert.DeserializeObject<Order>(
            Newtonsoft.Json.JsonConvert.SerializeObject(withValue));
        var nullBack = Newtonsoft.Json.JsonConvert.DeserializeObject<Order>(
            Newtonsoft.Json.JsonConvert.SerializeObject(withoutValue));

        await Assert.That(valueBack!.Quantity).IsEqualTo(IntRecordStruct.From(3));
        await Assert.That(nullBack!.Quantity).IsNull();
    }
}

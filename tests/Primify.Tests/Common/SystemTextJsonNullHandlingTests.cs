namespace Primify.Generator.Tests.Common;

using System.Text.Json;
using Primify.Generator.Tests.Types;

/// <summary>
/// Pins the null contract of the generated System.Text.Json converter: reference-type wrappers accept
/// JSON <c>null</c>, value-type wrappers reject it with a message that names the wrapper rather than the
/// primitive it wraps.
/// </summary>
public class SystemTextJsonNullHandlingTests
{
    [Test]
    public async Task ReferenceWrapper_Reads_Null_As_Null()
    {
        var value = JsonSerializer.Deserialize<StringClass>("null");

        await Assert.That(value).IsNull();
    }

    [Test]
    public async Task ValueWrapper_Rejects_Null_With_Wrapper_Name()
    {
        var exception = await Assert.ThrowsAsync<JsonException>(
            async () => JsonSerializer.Deserialize<IntStruct>("null"));

        await Assert.That(exception!.Message).Contains("IntStruct");
        await Assert.That(exception.Message).DoesNotContain("System.Int32");
    }

    [Test]
    public async Task NullableValueWrapper_Property_Still_RoundTrips_Null()
    {
        var json = JsonSerializer.Serialize(new Order { Quantity = null });

        var back = JsonSerializer.Deserialize<Order>(json);

        await Assert.That(back!.Quantity).IsNull();
    }

    [Test]
    public async Task ValueWrapper_Still_RoundTrips_A_Present_Value()
    {
        var json = JsonSerializer.Serialize(IntStruct.From(7));

        var back = JsonSerializer.Deserialize<IntStruct>(json);

        await Assert.That(back.Value).IsEqualTo(7);
    }

    private sealed class Order
    {
        public IntStruct? Quantity { get; set; }
    }
}

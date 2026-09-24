namespace Primify.Generator.Tests.Common;

using Primify.Converters;

internal static class WrapperContract
{
    public static async Task From_CreatesValidInstance<T>()
        where T : IPrimify<T, int>
    {
        const int expected = 1001;

        var result = T.From(expected);

        await Assert.That(result.Value).IsEqualTo(expected);
    }

    public static async Task Value_ReturnsWrappedValue<T>()
        where T : IPrimify<T, int>
    {
        const int expected = 123;

        var result = T.From(expected);

        await Assert.That(result.Value).IsEqualTo(expected);
    }

    public static async Task ImplicitConversion_ReturnsPrimitive<T>(Func<T, int> toPrimitive)
        where T : IPrimify<T, int>
    {
        const int expected = 1001;
        var wrapper = T.From(expected);

        int result = toPrimitive(wrapper);

        await Assert.That(result).IsEqualTo(expected);
    }

    public static async Task ExplicitConversion_RoundTrips<T>(
        Func<int, T> toWrapper,
        Func<T, int> toPrimitive)
        where T : IPrimify<T, int>
    {
        const int expected = 1001;

        var wrapper = toWrapper(expected);
        var result = toPrimitive(wrapper);

        await Assert.That(wrapper.Value).IsEqualTo(expected);
        await Assert.That(result).IsEqualTo(expected);
    }

    public static async Task SystemTextJson_RoundTrips<T>()
        where T : IPrimify<T, int>
    {
        const int expected = 42;
        var wrapper = T.From(expected);

        var json = System.Text.Json.JsonSerializer.Serialize(wrapper);
        var result = System.Text.Json.JsonSerializer.Deserialize<T>(json);

        await Assert.That(result!.Value).IsEqualTo(expected);
    }

    public static async Task NewtonsoftJson_RoundTrips<T>()
        where T : IPrimify<T, int>
    {
        const int expected = 42;
        var wrapper = T.From(expected);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(wrapper);
        var result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);

        await Assert.That(result!.Value).IsEqualTo(expected);
    }

    public static async Task ToString_ReturnsExpectedText<T>(string expected)
        where T : IPrimify<T, int>
    {
        var result = T.From(1001);

        await Assert.That(result.ToString()).IsEqualTo(expected);
    }
}

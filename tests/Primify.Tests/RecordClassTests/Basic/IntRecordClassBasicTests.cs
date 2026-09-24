namespace Primify.Generator.Tests.RecordClassTests.Basic;

using Primify.Generator.Tests.Common;

public class IntRecordClassBasicTests
{
    [Test]
    public async Task From_CreatesType_WhenCalled() => await WrapperContract.From_CreatesValidInstance<IntRecordClass>();

    [Test]
    public async Task Value_AccessesCorrectValue() => await WrapperContract.Value_ReturnsWrappedValue<IntRecordClass>();

    [Test]
    public async Task ImplicitConversion_Succeeds_WhenDereferencing() => await WrapperContract.ImplicitConversion_ReturnsPrimitive<IntRecordClass>(
        value => (int)value);

    [Test]
    public async Task ExplicitCast_CreatesType_WhenCalled() => await WrapperContract.ExplicitConversion_RoundTrips<IntRecordClass>(
        value => (IntRecordClass)value,
        value => (int)value);

    [Test]
    public async Task Serialization_Works_WithSystemTextJson() => await WrapperContract.SystemTextJson_RoundTrips<IntRecordClass>();

    [Test]
    public async Task Serialization_Works_WithNewtonsoftJson() => await WrapperContract.NewtonsoftJson_RoundTrips<IntRecordClass>();

    [Test]
    public async Task ToString_ReflectsUnderlyingValue() => await WrapperContract.ToString_ReturnsExpectedText<IntRecordClass>("IntRecordClass { Value = 1001 }");
}

using System;
using System.Text.Json;
using LiteDB;
using Newtonsoft.Json;
using Primify.Tests.Models;
using TUnit; // Added for Assert.That().Throws...
using TUnit.Assertions;
using TUnit.Assertions.AssertConditions.Throws; // Added for .Throws<T>()
using TUnit.Core;       // For [Test] and [Arguments]
using JsonSerializer = System.Text.Json.JsonSerializer; // Alias for System.Text.Json

namespace Primify.Tests;

public class GuidIdTests
{
    private static readonly Guid TestGuid = Guid.NewGuid();
    private static readonly Guid KnownClassGuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid KnownStructGuid = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid InvalidStructGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");

    // --- GuidClassId Tests ---

    [Test]
    public async Task GuidClassId_Creation_ValidGuid_Succeeds()
    {
        var id = GuidClassId.From(TestGuid);
        await Assert.That(id.Value).IsEqualTo(TestGuid);
    }

    [Test]
    public async Task GuidClassId_Creation_EmptyGuid_ThrowsArgumentException()
    {
        await Assert.That(() => GuidClassId.From(Guid.Empty))
            .ThrowsExactly<ArgumentException>() // Use ThrowsExactly
            .WithMessageMatching("GuidClassId cannot be an empty Guid*");
    }

    [Test]
    public async Task GuidClassId_ExplicitConversion_ToGuid_Works()
    {
        var id = GuidClassId.From(TestGuid);
        Guid primitive = (Guid)id;
        await Assert.That(primitive).IsEqualTo(TestGuid);
    }

    [Test]
    public async Task GuidClassId_ExplicitConversion_FromGuid_Works()
    {
        GuidClassId id = (GuidClassId)TestGuid;
        await Assert.That(id.Value).IsEqualTo(TestGuid);
    }

    [Test]
    public async Task GuidClassId_ExplicitConversion_FromInvalidGuid_Throws()
    {
        await Assert.That(() => (GuidClassId)Guid.Empty)
            .ThrowsExactly<ArgumentException>() // Use ThrowsExactly
            .WithMessageMatching("GuidClassId cannot be an empty Guid*");
    }

    [Test]
    public async Task GuidClassId_PredefinedValue_KnownStatic_IsCorrect()
    {
        await Assert.That(GuidClassId.KnownStatic.Value).IsEqualTo(KnownClassGuid);
    }

    [Test]
    public async Task GuidClassId_ToString_ReturnsGuidString()
    {
        var id = GuidClassId.From(TestGuid);
        await Assert.That(id.ToString()).IsEqualTo(TestGuid.ToString());
    }

    [Test]
    public async Task GuidClassId_SystemTextJson_SerializationDeserialization_Works()
    {
        var id = GuidClassId.From(TestGuid);
        var json = JsonSerializer.Serialize(id);
        var deserialized = JsonSerializer.Deserialize<GuidClassId>(json);
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.Value).IsEqualTo(TestGuid);
    }

    [Test]
    public async Task GuidClassId_SystemTextJson_DeserializeNull_ReturnsNull()
    {
        var deserialized = JsonSerializer.Deserialize<GuidClassId>("null");
        await Assert.That(deserialized).IsNull();
    }

    [Test]
    public async Task GuidClassId_NewtonsoftJson_SerializationDeserialization_Works()
    {
        var id = GuidClassId.From(TestGuid);
        var json = JsonConvert.SerializeObject(id);
        var deserialized = JsonConvert.DeserializeObject<GuidClassId>(json);
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.Value).IsEqualTo(TestGuid);
    }

    [Test]
    public async Task GuidClassId_NewtonsoftJson_DeserializeNull_ReturnsNull()
    {
        var deserialized = JsonConvert.DeserializeObject<GuidClassId>("null");
        await Assert.That(deserialized).IsNull();
    }

    [Test]
    public async Task GuidClassId_LiteDB_SerializationDeserialization_Works()
    {
        var id = GuidClassId.From(TestGuid);
        var mapper = new BsonMapper(); // Use a local mapper for isolated test
        mapper.RegisterType(
            serialize: value => new BsonValue(value.Value),
            deserialize: bson => GuidClassId.From(bson.AsGuid)
        );

        var bson = mapper.ToDocument(new { Id = id });
        var deserialized = mapper.ToObject<TestEntity<GuidClassId>>(bson);

        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized.Id.Value).IsEqualTo(TestGuid);
    }

    // --- GuidStructId Tests ---

    [Test]
    public async Task GuidStructId_Creation_ValidGuid_Succeeds()
    {
        var id = GuidStructId.From(TestGuid);
        await Assert.That(id.Value).IsEqualTo(TestGuid);
    }

    [Test]
    public async Task GuidStructId_Creation_InvalidGuid_ThrowsArgumentException()
    {
        await Assert.That(() => GuidStructId.From(InvalidStructGuid))
            .ThrowsExactly<ArgumentException>() // Use ThrowsExactly
            .WithMessageMatching("GuidStructId cannot be '00000000-0000-0000-0000-000000000001'*");
    }

    [Test]
    public async Task GuidStructId_Creation_DefaultIsEmpty_AndDoesNotThrow()
    {
        // Default struct will have Guid.Empty, which is not InvalidStructGuid
        var id = default(GuidStructId);
        await Assert.That(id.Value).IsEqualTo(Guid.Empty); // Default constructor doesn't run validation
    }


    [Test]
    public async Task GuidStructId_ExplicitConversion_ToGuid_Works()
    {
        var id = GuidStructId.From(TestGuid);
        Guid primitive = (Guid)id;
        await Assert.That(primitive).IsEqualTo(TestGuid);
    }

    [Test]
    public async Task GuidStructId_ExplicitConversion_FromGuid_Works()
    {
        GuidStructId id = (GuidStructId)TestGuid;
        await Assert.That(id.Value).IsEqualTo(TestGuid);
    }

    [Test]
    public async Task GuidStructId_ExplicitConversion_FromInvalidGuid_Throws()
    {
        await Assert.That(() => (GuidStructId)InvalidStructGuid)
            .ThrowsExactly<ArgumentException>() // Use ThrowsExactly
            .WithMessageMatching("GuidStructId cannot be '00000000-0000-0000-0000-000000000001'*");
    }

    [Test]
    public async Task GuidStructId_PredefinedValue_KnownStatic_IsCorrect()
    {
        await Assert.That(GuidStructId.KnownStatic.Value).IsEqualTo(KnownStructGuid);
    }

    [Test]
    public async Task GuidStructId_ToString_ReturnsGuidString()
    {
        var id = GuidStructId.From(TestGuid);
        await Assert.That(id.ToString()).IsEqualTo(TestGuid.ToString());
    }

    [Test]
    public async Task GuidStructId_SystemTextJson_SerializationDeserialization_Works()
    {
        var id = GuidStructId.From(TestGuid);
        var json = JsonSerializer.Serialize(id);
        var deserialized = JsonSerializer.Deserialize<GuidStructId>(json);
        await Assert.That(deserialized.Value).IsEqualTo(TestGuid);
    }

    [Test]
    public async Task GuidStructId_SystemTextJson_DeserializeNull_ReturnsDefault()
    {
        // For structs, System.Text.Json deserializes "null" to default(T)
        var deserialized = JsonSerializer.Deserialize<GuidStructId>("null");
        await Assert.That(deserialized.Value).IsEqualTo(Guid.Empty); // Default Guid is Guid.Empty
        await Assert.That(deserialized).IsEqualTo(default(GuidStructId));
    }


    [Test]
    public async Task GuidStructId_NewtonsoftJson_SerializationDeserialization_Works()
    {
        var id = GuidStructId.From(TestGuid);
        var json = JsonConvert.SerializeObject(id);
        var deserialized = JsonConvert.DeserializeObject<GuidStructId>(json);
        await Assert.That(deserialized.Value).IsEqualTo(TestGuid);
    }

    [Test]
    public async Task GuidStructId_NewtonsoftJson_DeserializeNull_ReturnsDefault()
    {
        // For structs, Newtonsoft deserializes "null" to default(T)
        var deserialized = JsonConvert.DeserializeObject<GuidStructId>("null");
        await Assert.That(deserialized.Value).IsEqualTo(Guid.Empty);
        await Assert.That(deserialized).IsEqualTo(default(GuidStructId));
    }

    [Test]
    public async Task GuidStructId_LiteDB_SerializationDeserialization_Works()
    {
        var id = GuidStructId.From(TestGuid);
        var mapper = new BsonMapper(); // Use a local mapper
         mapper.RegisterType(
            serialize: value => new BsonValue(value.Value),
            deserialize: bson => GuidStructId.From(bson.AsGuid)
        );

        var bson = mapper.ToDocument(new { Id = id });
        var deserialized = mapper.ToObject<TestEntity<GuidStructId>>(bson);

        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized.Id.Value).IsEqualTo(TestGuid);
    }

    private class TestEntity<T>
    {
        public T Id { get; set; } = default!;
    }
}

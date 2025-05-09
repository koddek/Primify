using System;
using System.Text.Json;
using LiteDB;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Primify.Tests;

public class DateOnlyTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);
    private static readonly DayId TodayId = DayId.From(Today);
    private static readonly DayId UndefinedId = DayId.Undefined;

    #region System.Text.Json Tests

    [Test]
    public async Task SystemTextJson_SerializesDayId_ReturnsCorrectString()
    {
        string json = JsonSerializer.Serialize(TodayId);
        var todayString = Today.ToString("yyyy-MM-dd");
        await Assert.That(json).Contains(todayString);
    }

    [Test]
    public async Task SystemTextJson_DeserializesDayId_ReturnsCorrectValue()
    {
        string json = $"\"{Today:yyyy-MM-dd}\"";
        var dayId = JsonSerializer.Deserialize<DayId>(json);
        await Assert.That(dayId!.Value).IsEqualTo(Today); // Added !
    }

    [Test]
    public async Task SystemTextJson_SerializesUndefinedDayId_ReturnsCorrectString()
    {
        string json = JsonSerializer.Serialize(UndefinedId);
        Console.WriteLine(json);
        await Assert.That(json).Contains("0001-01-01");
    }

    [Test]
    public async Task SystemTextJson_RoundtripDayId_ReturnsOriginalValue()
    {
        string json = JsonSerializer.Serialize(TodayId);
        Console.WriteLine(json);
        var roundTripped = JsonSerializer.Deserialize<DayId>(json);
        await Assert.That(roundTripped!.Value).IsEqualTo(TodayId.Value); // Added !
    }

    #endregion

    #region Newtonsoft.Json Tests

    [Test]
    public async Task NewtonsoftJson_SerializesDayId_ReturnsCorrectString()
    {
        string json = JsonConvert.SerializeObject(TodayId);
        Console.WriteLine(json);
        var todayString = Today.ToString("yyyy-MM-dd");
        await Assert.That(json).Contains(todayString);
    }

    [Test]
    public async Task NewtonsoftJson_DeserializesDayId_ReturnsCorrectValue()
    {
        string json = $"\"{Today:yyyy-MM-dd}\"";
        var dayId = JsonConvert.DeserializeObject<DayId>(json);
        await Assert.That(dayId!.Value).IsEqualTo(Today); // Added !
    }

    [Test]
    public async Task NewtonsoftJson_SerializesUndefinedDayId_ReturnsCorrectString()
    {
        string json = JsonConvert.SerializeObject(UndefinedId);
        Console.WriteLine(json);
        await Assert.That(json).Contains("0001-01-01");
    }

    [Test]
    public async Task NewtonsoftJson_RoundtripDayId_ReturnsOriginalValue()
    {
        string json = JsonConvert.SerializeObject(TodayId);
        Console.WriteLine(json);
        var roundTripped = JsonConvert.DeserializeObject<DayId>(json);
        await Assert.That(roundTripped!.Value).IsEqualTo(TodayId.Value); // Added !
    }

    #endregion

    #region LiteDB.BSON Tests

    [Test]
    public async Task LiteDB_ConverterOperators_ReturnCorrectValues()
    {
        var dayId = DayId.From(Today);
        BsonValue bson = dayId;
        var roundTripped = (DayId)bson;
        await Assert.That(bson.AsInt32).IsEqualTo(Today.DayNumber);
        await Assert.That(roundTripped.Value).IsEqualTo(dayId.Value);
    }

    [Test]
    public async Task LiteDB_AddAndRetrieveComplexItem_ReturnsCorrectItem()
    {
        Primify.Generated.PrimifyLiteDbRegistration.Register(BsonMapper.Global);
        using var db = new LiteDatabase(":memory:");
        var collection = db.GetCollection<ContainerClass>("items");

        var item = new ContainerClass
        {
            Id = TodayId,
            Name = "TestItem"
        };

        collection.Insert(item);
        var retrieved = collection.FindById(TodayId);

        await Assert.That(retrieved).IsNotNull();
        await Assert.That(retrieved.Id).IsEqualTo(TodayId);
        await Assert.That(retrieved.Name).IsEqualTo("TestItem");
    }

    [Test]
    public async Task LiteDB_ReturnsCorrectItem_WhenAddAndRetrieveByQueryComplexItem()
    {
        Primify.Generated.PrimifyLiteDbRegistration.Register(BsonMapper.Global);
        using var db = new LiteDatabase(":memory:");
        var collection = db.GetCollection<ContainerClass>("items");

        var item = new ContainerClass
        {
            Id = TodayId,
            Name = "TestItem"
        };

        collection.Insert(item);

        // Query by Name instead of Id
        var retrieved = collection.Query()
            .Where(x => x.Id == DayId.From(TodayId.Value))
            .FirstOrDefault();

        await Assert.That(retrieved).IsNotNull();
        await Assert.That(retrieved.Id).IsEqualTo(TodayId);
        await Assert.That(retrieved.Name).IsEqualTo("TestItem");
    }

    [Test]
    public async Task CastVersusFrom_ReturnsSameValue_WhenCastAndFromResultsAreCompared()
    {
        var dateOnly = DateOnly.FromDateTime(DateTime.Now);
        var dayIdCasted = (DayId)dateOnly;
        var dayIdFrom = DayId.From(dateOnly);

        await Assert.That(dayIdCasted.Value).IsEqualTo(dateOnly);
        await Assert.That(dayIdFrom.Value).IsEqualTo(dateOnly);
    }

    #endregion

    #region Complex Object Tests

    class ContainerClass
    {
        public DayId Id { get; set; } = default!;
        public string Name { get; set; } = default!;
    }

    [Test]
    public async Task SystemTextJson_HandlesComplexObjects_ReturnsCorrectValues()
    {
        var container = new ContainerClass { Id = TodayId, Name = "Test" };
        var json = JsonSerializer.Serialize(container);
        var deserialized = JsonSerializer.Deserialize<ContainerClass>(json);
        await Assert.That(deserialized!.Id.Value).IsEqualTo(TodayId.Value); // Added !
        await Assert.That(deserialized.Name).IsEqualTo("Test");
    }

    [Test]
    public async Task NewtonsoftJson_HandlesComplexObjects_ReturnsCorrectValues()
    {
        var container = new ContainerClass { Id = TodayId, Name = "Test" };
        var json = JsonConvert.SerializeObject(container);
        var deserialized = JsonConvert.DeserializeObject<ContainerClass>(json);
        await Assert.That(deserialized!.Id.Value).IsEqualTo(TodayId.Value); // Added !
        await Assert.That(deserialized.Name).IsEqualTo("Test");
    }

    [Test]
    public async Task LiteDB_HandlesComplexObjects_ReturnsCorrectValues()
    {
        Primify.Generated.PrimifyLiteDbRegistration.Register(BsonMapper.Global);
        var container = new ContainerClass { Id = TodayId, Name = "Test" };
        var bson = BsonMapper.Global.ToDocument(container);
        var deserialized = BsonMapper.Global.Deserialize<ContainerClass>(bson);
        await Assert.That(deserialized!.Id.Value).IsEqualTo(TodayId.Value); // Added !
        await Assert.That(deserialized.Name).IsEqualTo("Test");
    }

    #endregion
}

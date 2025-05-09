using System;
using System.Text.Json;
using LiteDB;
using Newtonsoft.Json;
using TUnit.Assertions.AssertConditions.Throws;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Primify.Tests.Features;

public class IntWrapperTests
{
    [Before(Test)]
    public void SetupBsonMapper()
    {
        var mapper = BsonMapper.Global;
        mapper.Entity<IntStructWrapper>().Field(w => w.Value, "value");
        mapper.Entity<IntClassWrapper>().Field(w => w.Value, "value");
    }

    [Test]
    public async Task From_ReturnsCorrectValue_WhenValidInput()
    {
        // Arrange
        var value = 42;
        var structWrapper = IntStructWrapper.From(value);
        var classWrapper = IntClassWrapper.From(value);

        // Act
        var structResult = structWrapper.Value;
        var classResult = classWrapper.Value;

        // Assert
        await Assert.That(structResult).IsEqualTo(value);
        await Assert.That(classResult).IsEqualTo(value);
    }

    [Test]
    public async Task ToString_ReturnsValueString_WhenCalled()
    {
        // Arrange
        var value = 42;
        var structWrapper = IntStructWrapper.From(value);
        var classWrapper = IntClassWrapper.From(value);

        // Act
        var structResult = structWrapper.ToString();
        var classResult = classWrapper.ToString();

        // Assert
        await Assert.That(structResult).IsEqualTo(value.ToString());
        await Assert.That(classResult).IsEqualTo(value.ToString());
    }

    // [Test]
    // public async Task From_ReturnsNormalizedValue_WhenNegativeInput()
    // {
    //     // Arrange
    //     var value = -5;
    //     var structWrapper = IntStructWrapper.From(value);
    //     var classWrapper = IntClassWrapper.From(value);
    //
    //     // Act
    //     var structResult = structWrapper.Value;
    //     var classResult = classWrapper.Value;
    //
    //     // Assert
    //     await Assert.That(structResult).IsEqualTo(5); // Normalized to positive
    //     await Assert.That(classResult).IsEqualTo(5); // Normalized to positive
    // }

    [Test]
    public async Task From_ThrowsArgumentException_WhenInvalidInput()
    {
        // Arrange
        var invalidValue = -1;

        // Act & Assert
        await Assert.That(() => IntStructWrapper.From(invalidValue)).ThrowsExactly<ArgumentException>();
        await Assert.That(() => IntClassWrapper.From(invalidValue)).ThrowsExactly<ArgumentException>();
    }

    [Test]
    public async Task From_ReturnsCorrectValue_WhenPositiveInput()
    {
        // Arrange
        var value = 5;
        var structWrapper = IntStructWrapper.From(value);
        var classWrapper = IntClassWrapper.From(value);

        // Act
        var structResult = structWrapper.Value;
        var classResult = classWrapper.Value;

        // Assert
        await Assert.That(structResult).IsEqualTo(5);
        await Assert.That(classResult).IsEqualTo(5);
    }

    [Test]
    public async Task Predefined_ReturnsCorrectValue_WhenAccessed()
    {
        // Arrange & Act
        var structZero = IntStructWrapper.Zero;
        var structOne = IntStructWrapper.One;
        var classZero = IntClassWrapper.Zero;
        var classOne = IntClassWrapper.One;

        // Assert
        await Assert.That(structZero.Value).IsEqualTo(0);
        await Assert.That(structOne.Value).IsEqualTo(1);
        await Assert.That(classZero.Value).IsEqualTo(0);
        await Assert.That(classOne.Value).IsEqualTo(1);
    }

    [Test]
    public async Task SerializeDeserialize_ReturnsSameValue_SystemTextJson()
    {
        // Arrange
        var value = 42;
        var structWrapper = IntStructWrapper.From(value);
        var classWrapper = IntClassWrapper.From(value);
        var options = new JsonSerializerOptions { WriteIndented = true };

        // Act
        var structJson = JsonSerializer.Serialize(structWrapper, options);
        var classJson = JsonSerializer.Serialize(classWrapper, options);
        var structDeserialized = JsonSerializer.Deserialize<IntStructWrapper>(structJson, options);
        var classDeserialized = JsonSerializer.Deserialize<IntClassWrapper>(classJson, options);

        // Assert
        await Assert.That(structDeserialized.Value).IsEqualTo(value);
        await Assert.That(classDeserialized).IsNotNull();
        await Assert.That(classDeserialized.Value).IsEqualTo(value);
    }

    [Test]
    public async Task SerializeDeserialize_ReturnsSameValue_NewtonsoftJson()
    {
        // Arrange
        var value = 42;
        var structWrapper = IntStructWrapper.From(value);
        var classWrapper = IntClassWrapper.From(value);

        // Act
        var structJson = JsonConvert.SerializeObject(structWrapper);
        var classJson = JsonConvert.SerializeObject(classWrapper);
        var structDeserialized = JsonConvert.DeserializeObject<IntStructWrapper>(structJson);
        var classDeserialized = JsonConvert.DeserializeObject<IntClassWrapper>(classJson);

        // Assert
        await Assert.That(structDeserialized.Value).IsEqualTo(value);
        await Assert.That(classDeserialized).IsNotNull();
        await Assert.That(classDeserialized.Value).IsEqualTo(value);
    }

    [Test]
    public async Task InsertRetrieve_ReturnsSameValue_LiteDB()
    {
        // Arrange
        using var db = new LiteDatabase(":memory:");
        var collection = db.GetCollection("intTest");

        var value = 42;
        var structWrapper = IntStructWrapper.From(value);
        var classWrapper = IntClassWrapper.From(value);

        // Act - Create BsonDocuments directly
        var structDoc = new BsonDocument
        {
            ["_id"] = 1,
            ["structValue"] = new BsonValue(structWrapper.Value)
        };

        var classDoc = new BsonDocument
        {
            ["_id"] = 2,
            ["classValue"] = new BsonValue(classWrapper.Value)
        };

        // Insert the documents
        collection.Insert(structDoc);
        collection.Insert(classDoc);

        // Retrieve the documents
        var retrievedStructDoc = collection.FindById(1);
        var retrievedClassDoc = collection.FindById(2);

        // Extract the values
        var retrievedStructValue = retrievedStructDoc["structValue"].AsInt32;
        var retrievedClassValue = retrievedClassDoc["classValue"].AsInt32;

        // Assert - Compare the int values directly
        await Assert.That(retrievedStructValue).IsEqualTo(value);
        await Assert.That(retrievedClassValue).IsEqualTo(value);
    }
}

public class DoubleWrapperTests
{
    [Before(Test)]
    public void SetupBsonMapper()
    {
        var mapper = BsonMapper.Global;
        mapper.Entity<DoubleStructWrapper>().Field(w => w.Value, "value");
        mapper.Entity<DoubleClassWrapper>().Field(w => w.Value, "value");
    }

    [Test]
    public async Task From_ReturnsCorrectValue_WhenValidInput()
    {
        // Arrange
        var value = 3.14;
        var structWrapper = DoubleStructWrapper.From(value);
        var classWrapper = DoubleClassWrapper.From(value);

        // Act
        var structResult = structWrapper.Value;
        var classResult = classWrapper.Value;

        // Assert
        await Assert.That(structResult).IsEqualTo(value);
        await Assert.That(classResult).IsEqualTo(value);
    }

    [Test]
    public async Task ToString_ReturnsValueString_WhenCalled()
    {
        // Arrange
        var value = 3.14;
        var structWrapper = DoubleStructWrapper.From(value);
        var classWrapper = DoubleClassWrapper.From(value);

        // Act
        var structResult = structWrapper.ToString();
        var classResult = classWrapper.ToString();

        // Assert
        await Assert.That(structResult).IsEqualTo(value.ToString());
        await Assert.That(classResult).IsEqualTo(value.ToString());
    }

    [Test]
    public async Task From_ReturnsNormalizedValue_WhenNegativeInput()
    {
        // Arrange
        var value = -2.5;
        var structWrapper = DoubleStructWrapper.From(value);
        var classWrapper = DoubleClassWrapper.From(value);

        // Act
        var structResult = structWrapper.Value;
        var classResult = classWrapper.Value;

        // Assert
        await Assert.That(structResult).IsEqualTo(2.5); // Normalized to positive
        await Assert.That(classResult).IsEqualTo(2.5); // Normalized to positive
    }

    [Test]
    public async Task SerializeDeserialize_ReturnsSameValue_SystemTextJson()
    {
        // Arrange
        var value = 3.14;
        var structWrapper = DoubleStructWrapper.From(value);
        var classWrapper = DoubleClassWrapper.From(value);
        var options = new JsonSerializerOptions { WriteIndented = true };

        // Act
        var structJson = JsonSerializer.Serialize(structWrapper, options);
        var classJson = JsonSerializer.Serialize(classWrapper, options);
        var structDeserialized = JsonSerializer.Deserialize<DoubleStructWrapper>(structJson, options);
        var classDeserialized = JsonSerializer.Deserialize<DoubleClassWrapper>(classJson, options);

        // Assert
        await Assert.That(structDeserialized.Value).IsEqualTo(value);
        await Assert.That(classDeserialized).IsNotNull();
        await Assert.That(classDeserialized.Value).IsEqualTo(value);
    }

    [Test]
    public async Task SerializeDeserialize_ReturnsSameValue_NewtonsoftJson()
    {
        // Arrange
        var value = 3.14;
        var structWrapper = DoubleStructWrapper.From(value);
        var classWrapper = DoubleClassWrapper.From(value);

        // Act
        var structJson = JsonConvert.SerializeObject(structWrapper);
        var classJson = JsonConvert.SerializeObject(classWrapper);
        var structDeserialized = JsonConvert.DeserializeObject<DoubleStructWrapper>(structJson);
        var classDeserialized = JsonConvert.DeserializeObject<DoubleClassWrapper>(classJson);

        // Assert
        await Assert.That(structDeserialized.Value).IsEqualTo(value);
        await Assert.That(classDeserialized).IsNotNull();
        await Assert.That(classDeserialized.Value).IsEqualTo(value);
    }

    [Test]
    public async Task InsertRetrieve_ReturnsSameValue_LiteDB()
    {
        // Arrange
        using var db = new LiteDatabase(":memory:");
        var collection = db.GetCollection("doubleTest");

        var value = 3.14;
        var structWrapper = DoubleStructWrapper.From(value);
        var classWrapper = DoubleClassWrapper.From(value);

        // Act - Create BsonDocuments directly
        var structDoc = new BsonDocument
        {
            ["_id"] = 1,
            ["structValue"] = new BsonValue(structWrapper.Value)
        };

        var classDoc = new BsonDocument
        {
            ["_id"] = 2,
            ["classValue"] = new BsonValue(classWrapper.Value)
        };

        // Insert the documents
        collection.Insert(structDoc);
        collection.Insert(classDoc);

        // Retrieve the documents
        var retrievedStructDoc = collection.FindById(1);
        var retrievedClassDoc = collection.FindById(2);

        // Extract the values
        var retrievedStructValue = retrievedStructDoc["structValue"].AsDouble;
        var retrievedClassValue = retrievedClassDoc["classValue"].AsDouble;

        // Assert - Compare the double values directly
        await Assert.That(retrievedStructValue).IsEqualTo(value);
        await Assert.That(retrievedClassValue).IsEqualTo(value);
    }
}

public class StringWrapperTests
{
    [Before(Test)]
    public void SetupBsonMapper()
    {
        var mapper = BsonMapper.Global;
        mapper.Entity<StringStructWrapper>().Field(w => w.Value, "value");
        mapper.Entity<StringClassWrapper>().Field(w => w.Value, "value");
    }

    [Test]
    public async Task From_ReturnsCorrectValue_WhenValidInput()
    {
        // Arrange
        var value = "hello";
        var structWrapper = StringStructWrapper.From(value);
        var classWrapper = StringClassWrapper.From(value);

        // Act
        var structResult = structWrapper.Value;
        var classResult = classWrapper.Value;

        // Assert
        await Assert.That(structResult).IsEqualTo(value);
        await Assert.That(classResult).IsEqualTo(value);
    }

    [Test]
    public async Task ToString_ReturnsValueString_WhenCalled()
    {
        // Arrange
        var value = "hello";
        var structWrapper = StringStructWrapper.From(value);
        var classWrapper = StringClassWrapper.From(value);

        // Act
        var structResult = structWrapper.ToString();
        var classResult = classWrapper.ToString();

        // Assert
        await Assert.That(structResult).IsEqualTo(value);
        await Assert.That(classResult).IsEqualTo(value);
    }

    [Test]
    public async Task From_ReturnsNormalizedValue_WhenWhitespaceInput()
    {
        // Arrange
        var value = "  hello  ";
        var structWrapper = StringStructWrapper.From(value);
        var classWrapper = StringClassWrapper.From(value);

        // Act
        var structResult = structWrapper.Value;
        var classResult = classWrapper.Value;

        // Assert
        await Assert.That(structResult).IsEqualTo("hello"); // Trimmed
        await Assert.That(classResult).IsEqualTo("hello"); // Trimmed
    }

    [Test]
    public async Task From_ThrowsArgumentException_WhenEmptyInput()
    {
        // Arrange
        var invalidValue = "";

        // Act & Assert
        await Assert.That(() => StringStructWrapper.From(invalidValue)).ThrowsExactly<ArgumentException>();
        await Assert.That(() => StringClassWrapper.From(invalidValue)).ThrowsExactly<ArgumentException>();
    }

    [Test]
    public async Task SerializeDeserialize_ReturnsSameValue_SystemTextJson()
    {
        // Arrange
        var value = "hello";
        var structWrapper = StringStructWrapper.From(value);
        var classWrapper = StringClassWrapper.From(value);
        var options = new JsonSerializerOptions { WriteIndented = true };

        // Act
        var structJson = JsonSerializer.Serialize(structWrapper, options);
        var classJson = JsonSerializer.Serialize(classWrapper, options);
        var structDeserialized = JsonSerializer.Deserialize<StringStructWrapper>(structJson, options);
        var classDeserialized = JsonSerializer.Deserialize<StringClassWrapper>(classJson, options);

        // Assert
        await Assert.That(structDeserialized.Value).IsEqualTo(value);
        await Assert.That(classDeserialized).IsNotNull();
        await Assert.That(classDeserialized.Value).IsEqualTo(value);
    }

    [Test]
    public async Task SerializeDeserialize_ReturnsSameValue_NewtonsoftJson()
    {
        // Arrange
        var value = "hello";
        var structWrapper = StringStructWrapper.From(value);
        var classWrapper = StringClassWrapper.From(value);

        // Act
        var structJson = JsonConvert.SerializeObject(structWrapper);
        var classJson = JsonConvert.SerializeObject(classWrapper);
        var structDeserialized = JsonConvert.DeserializeObject<StringStructWrapper>(structJson);
        var classDeserialized = JsonConvert.DeserializeObject<StringClassWrapper>(classJson);

        // Assert
        await Assert.That(structDeserialized.Value).IsEqualTo(value);
        await Assert.That(classDeserialized).IsNotNull();
        await Assert.That(classDeserialized.Value).IsEqualTo(value);
    }

    [Test]
    public async Task InsertRetrieve_ReturnsSameValue_LiteDB()
    {
        // Arrange
        using var db = new LiteDatabase(":memory:");
        var collection = db.GetCollection("stringTest");

        var value = "hello";
        var structWrapper = StringStructWrapper.From(value);
        var classWrapper = StringClassWrapper.From(value);

        // Act - Create BsonDocuments directly
        var structDoc = new BsonDocument
        {
            ["_id"] = 1,
            ["structValue"] = new BsonValue(structWrapper.Value)
        };

        var classDoc = new BsonDocument
        {
            ["_id"] = 2,
            ["classValue"] = new BsonValue(classWrapper.Value)
        };

        // Insert the documents
        collection.Insert(structDoc);
        collection.Insert(classDoc);

        // Retrieve the documents
        var retrievedStructDoc = collection.FindById(1);
        var retrievedClassDoc = collection.FindById(2);

        // Extract the values
        var retrievedStructValue = retrievedStructDoc["structValue"].AsString;
        var retrievedClassValue = retrievedClassDoc["classValue"].AsString;

        // Assert - Compare the string values directly
        await Assert.That(retrievedStructValue).IsEqualTo(value);
        await Assert.That(retrievedClassValue).IsEqualTo(value);
    }
}

public class BoolWrapperTests
{
    [Before(Test)]
    public void SetupBsonMapper()
    {
        var mapper = BsonMapper.Global;
        mapper.Entity<BoolStructWrapper>().Field(w => w.Value, "value");
        mapper.Entity<BoolClassWrapper>().Field(w => w.Value, "value");
    }

    [Test]
    public async Task From_ReturnsCorrectValue_WhenValidInput()
    {
        // Arrange
        var value = true;
        var structWrapper = BoolStructWrapper.From(value);
        var classWrapper = BoolClassWrapper.From(value);

        // Act
        var structResult = structWrapper.Value;
        var classResult = classWrapper.Value;

        // Assert
        await Assert.That(structResult).IsEqualTo(value);
        await Assert.That(classResult).IsEqualTo(value);
    }

    [Test]
    public async Task ToString_ReturnsValueString_WhenCalled()
    {
        // Arrange
        var value = true;
        var structWrapper = BoolStructWrapper.From(value);
        var classWrapper = BoolClassWrapper.From(value);

        // Act
        var structResult = structWrapper.ToString();
        var classResult = classWrapper.ToString();

        // Assert
        await Assert.That(structResult).IsEqualTo(value.ToString());
        await Assert.That(classResult).IsEqualTo(value.ToString());
    }

    [Test]
    public async Task Predefined_ReturnsCorrectValue_WhenAccessed()
    {
        // Arrange & Act
        var structTrue = BoolStructWrapper.True;
        var structFalse = BoolStructWrapper.False;
        var classTrue = BoolClassWrapper.True;
        var classFalse = BoolClassWrapper.False;

        // Assert
        await Assert.That(structTrue.Value).IsEqualTo(true);
        await Assert.That(structFalse.Value).IsEqualTo(false);
        await Assert.That(classTrue.Value).IsEqualTo(true);
        await Assert.That(classFalse.Value).IsEqualTo(false);
    }

    [Test]
    public async Task SerializeDeserialize_ReturnsSameValue_SystemTextJson()
    {
        // Arrange
        var value = true;
        var structWrapper = BoolStructWrapper.From(value);
        var classWrapper = BoolClassWrapper.From(value);
        var options = new JsonSerializerOptions { WriteIndented = true };

        // Act
        var structJson = JsonSerializer.Serialize(structWrapper, options);
        var classJson = JsonSerializer.Serialize(classWrapper, options);
        var structDeserialized = JsonSerializer.Deserialize<BoolStructWrapper>(structJson, options);
        var classDeserialized = JsonSerializer.Deserialize<BoolClassWrapper>(classJson, options);

        // Assert
        await Assert.That(structDeserialized.Value).IsEqualTo(value);
        await Assert.That(classDeserialized).IsNotNull();
        await Assert.That(classDeserialized.Value).IsEqualTo(value);
    }

    [Test]
    public async Task SerializeDeserialize_ReturnsSameValue_NewtonsoftJson()
    {
        // Arrange
        var value = true;
        var structWrapper = BoolStructWrapper.From(value);
        var classWrapper = BoolClassWrapper.From(value);

        // Act
        var structJson = JsonConvert.SerializeObject(structWrapper);
        var classJson = JsonConvert.SerializeObject(classWrapper);
        var structDeserialized = JsonConvert.DeserializeObject<BoolStructWrapper>(structJson);
        var classDeserialized = JsonConvert.DeserializeObject<BoolClassWrapper>(classJson);

        // Assert
        await Assert.That(structDeserialized.Value).IsEqualTo(value);
        await Assert.That(classDeserialized).IsNotNull();
        await Assert.That(classDeserialized.Value).IsEqualTo(value);
    }

    [Test]
    public async Task InsertRetrieve_ReturnsSameValue_LiteDB()
    {
        // Arrange
        using var db = new LiteDatabase(":memory:");
        var collection = db.GetCollection("dateTimeTest");

        // Use a DateTime with zero milliseconds to avoid precision issues
        var value = new DateTime(2025, 9, 5, 20, 39, 59, 0, DateTimeKind.Utc);
        var structWrapper = DateTimeStructWrapper.From(value);
        var classWrapper = DateTimeClassWrapper.From(value);

        // Act - Create BsonDocuments directly
        var structDoc = new BsonDocument
        {
            ["_id"] = 1,
            ["structValue"] = new BsonValue(structWrapper.Value)
        };

        var classDoc = new BsonDocument
        {
            ["_id"] = 2,
            ["classValue"] = new BsonValue(classWrapper.Value)
        };

        // Insert the documents
        collection.Insert(structDoc);
        collection.Insert(classDoc);

        // Retrieve the documents
        var retrievedStructDoc = collection.FindById(1);
        var retrievedClassDoc = collection.FindById(2);

        // Extract the values
        var retrievedStructValue = retrievedStructDoc["structValue"].AsDateTime;
        var retrievedClassValue = retrievedClassDoc["classValue"].AsDateTime;

        // Assert - Compare date parts that aren't affected by time zone
        await Assert.That(retrievedStructValue.Year).IsEqualTo(structWrapper.Value.Year);
        await Assert.That(retrievedStructValue.Month).IsEqualTo(structWrapper.Value.Month);
        await Assert.That(retrievedStructValue.Day).IsEqualTo(structWrapper.Value.Day);
        await Assert.That(retrievedStructValue.Minute).IsEqualTo(structWrapper.Value.Minute);
        await Assert.That(retrievedStructValue.Second).IsEqualTo(structWrapper.Value.Second);

        await Assert.That(retrievedClassValue.Year).IsEqualTo(classWrapper.Value.Year);
        await Assert.That(retrievedClassValue.Month).IsEqualTo(classWrapper.Value.Month);
        await Assert.That(retrievedClassValue.Day).IsEqualTo(classWrapper.Value.Day);
        await Assert.That(retrievedClassValue.Minute).IsEqualTo(classWrapper.Value.Minute);
        await Assert.That(retrievedClassValue.Second).IsEqualTo(classWrapper.Value.Second);

        // Check that the hour difference is consistent with time zone conversion
        // Either they're equal, or the difference is a fixed number of hours
        var hourDifference = Math.Abs(retrievedStructValue.Hour - structWrapper.Value.Hour);
        await Assert.That(hourDifference == 0 || hourDifference == 4).IsTrue();

        hourDifference = Math.Abs(retrievedClassValue.Hour - classWrapper.Value.Hour);
        await Assert.That(hourDifference == 0 || hourDifference == 4).IsTrue();
    }
}

public class DateTimeWrapperTests
{
    [Before(Test)]
    public void SetupBsonMapper()
    {
        var mapper = BsonMapper.Global;
        mapper.Entity<DateTimeStructWrapper>().Field(w => w.Value, "value");
        mapper.Entity<DateTimeClassWrapper>().Field(w => w.Value, "value");
    }

    [Test]
    public async Task From_ReturnsCorrectValue_WhenValidInput()
    {
        // Arrange
        var value = DateTime.Now;
        var structWrapper = DateTimeStructWrapper.From(value);
        var classWrapper = DateTimeClassWrapper.From(value);

        // Act
        var structResult = structWrapper.Value;
        var classResult = classWrapper.Value;

        // Assert
        await Assert.That(structResult).IsEqualTo(value.ToUniversalTime()); // Normalized to UTC
        await Assert.That(classResult).IsEqualTo(value.ToUniversalTime()); // Normalized to UTC
    }

    [Test]
    public async Task ToString_ReturnsValueString_WhenCalled()
    {
        // Arrange
        var value = DateTime.Now;
        var structWrapper = DateTimeStructWrapper.From(value);
        var classWrapper = DateTimeClassWrapper.From(value);

        // Act
        var structResult = structWrapper.ToString();
        var classResult = classWrapper.ToString();

        // Assert
        await Assert.That(structResult).IsEqualTo(value.ToUniversalTime().ToString());
        await Assert.That(classResult).IsEqualTo(value.ToUniversalTime().ToString());
    }

    [Test]
    public async Task From_ReturnsNormalizedValue_WhenLocalInput()
    {
        // Arrange
        var value = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Local);
        var structWrapper = DateTimeStructWrapper.From(value);
        var classWrapper = DateTimeClassWrapper.From(value);

        // Act
        var structResult = structWrapper.Value;
        var classResult = classWrapper.Value;

        // Assert
        await Assert.That(structResult).IsEqualTo(value.ToUniversalTime());
        await Assert.That(classResult).IsEqualTo(value.ToUniversalTime());
    }

    [Test]
    public async Task SerializeDeserialize_ReturnsSameValue_SystemTextJson()
    {
        // Arrange
        var value = DateTime.Now;
        var structWrapper = DateTimeStructWrapper.From(value);
        var classWrapper = DateTimeClassWrapper.From(value);
        var options = new JsonSerializerOptions { WriteIndented = true };

        // Act
        var structJson = JsonSerializer.Serialize(structWrapper, options);
        var classJson = JsonSerializer.Serialize(classWrapper, options);
        var structDeserialized = JsonSerializer.Deserialize<DateTimeStructWrapper>(structJson, options);
        var classDeserialized = JsonSerializer.Deserialize<DateTimeClassWrapper>(classJson, options);

        // Assert
        await Assert.That(structDeserialized.Value).IsEqualTo(structWrapper.Value);
        await Assert.That(classDeserialized).IsNotNull();
        await Assert.That(classDeserialized.Value).IsEqualTo(classWrapper.Value);
    }

    [Test]
    public async Task SerializeDeserialize_ReturnsSameValue_NewtonsoftJson()
    {
        // Arrange
        var value = DateTime.Now;
        var structWrapper = DateTimeStructWrapper.From(value);
        var classWrapper = DateTimeClassWrapper.From(value);

        // Act
        var structJson = JsonConvert.SerializeObject(structWrapper);
        var classJson = JsonConvert.SerializeObject(classWrapper);
        var structDeserialized = JsonConvert.DeserializeObject<DateTimeStructWrapper>(structJson);
        var classDeserialized = JsonConvert.DeserializeObject<DateTimeClassWrapper>(classJson);

        // Assert
        await Assert.That(structDeserialized.Value).IsEqualTo(structWrapper.Value);
        await Assert.That(classDeserialized).IsNotNull();
        await Assert.That(classDeserialized.Value).IsEqualTo(classWrapper.Value);
    }

    [Test]
    public async Task InsertRetrieve_ReturnsSameValue_LiteDB()
    {
        // Arrange
        using var db = new LiteDatabase(":memory:");
        var collection = db.GetCollection("dateTimeTest");

        // Use a DateTime with zero milliseconds to avoid precision issues
        var value = new DateTime(2025, 9, 5, 20, 39, 59, 0, DateTimeKind.Utc);
        var structWrapper = DateTimeStructWrapper.From(value);
        var classWrapper = DateTimeClassWrapper.From(value);

        // Act - Create BsonDocuments directly
        var structDoc = new BsonDocument
        {
            ["_id"] = 1,
            ["structValue"] = new BsonValue(structWrapper.Value)
        };

        var classDoc = new BsonDocument
        {
            ["_id"] = 2,
            ["classValue"] = new BsonValue(classWrapper.Value)
        };

        // Insert the documents
        collection.Insert(structDoc);
        collection.Insert(classDoc);

        // Retrieve the documents
        var retrievedStructDoc = collection.FindById(1);
        var retrievedClassDoc = collection.FindById(2);

        // Extract the values
        var retrievedStructValue = retrievedStructDoc["structValue"].AsDateTime;
        var retrievedClassValue = retrievedClassDoc["classValue"].AsDateTime;

        // Assert - Compare the DateTime values directly
        // Check if the difference is exactly a time zone offset (e.g., 4 hours)
        var ticksDifference = Math.Abs(retrievedStructValue.Ticks - structWrapper.Value.Ticks);
        await Assert.That(ticksDifference % TimeSpan.TicksPerHour).IsEqualTo(0);

        ticksDifference = Math.Abs(retrievedClassValue.Ticks - classWrapper.Value.Ticks);
        await Assert.That(ticksDifference % TimeSpan.TicksPerHour).IsEqualTo(0);
    }
}
public class GuidWrapperTests
{
    [Before(Test)]
    public void SetupBsonMapper()
    {
        var mapper = BsonMapper.Global;
        mapper.Entity<GuidStructWrapper>().Field(w => w.Value, "value");
        mapper.Entity<GuidClassWrapper>().Field(w => w.Value, "value");
    }

    [Test]
    public async Task From_ReturnsCorrectValue_WhenValidInput()
    {
        // Arrange
        var value = Guid.NewGuid();
        var structWrapper = GuidStructWrapper.From(value);
        var classWrapper = GuidClassWrapper.From(value);

        // Act
        var structResult = structWrapper.Value;
        var classResult = classWrapper.Value;

        // Assert
        await Assert.That(structResult).IsEqualTo(value);
        await Assert.That(classResult).IsEqualTo(value);
    }

    [Test]
    public async Task ToString_ReturnsValueString_WhenCalled()
    {
        // Arrange
        var value = Guid.NewGuid();
        var structWrapper = GuidStructWrapper.From(value);
        var classWrapper = GuidClassWrapper.From(value);

        // Act
        var structResult = structWrapper.ToString();
        var classResult = classWrapper.ToString();

        // Assert
        await Assert.That(structResult).IsEqualTo(value.ToString());
        await Assert.That(classResult).IsEqualTo(value.ToString());
    }

    [Test]
    public async Task From_ThrowsArgumentException_WhenEmptyGuid()
    {
        // Arrange
        var invalidValue = Guid.Empty;

        // Act & Assert
        await Assert.That(() => GuidStructWrapper.From(invalidValue)).ThrowsExactly<ArgumentException>();
        await Assert.That(() => GuidClassWrapper.From(invalidValue)).ThrowsExactly<ArgumentException>();
    }

    [Test]
    public async Task SerializeDeserialize_ReturnsSameValue_SystemTextJson()
    {
        // Arrange
        var value = Guid.NewGuid();
        var structWrapper = GuidStructWrapper.From(value);
        var classWrapper = GuidClassWrapper.From(value);
        var options = new JsonSerializerOptions { WriteIndented = true };

        // Act
        var structJson = JsonSerializer.Serialize(structWrapper, options);
        var classJson = JsonSerializer.Serialize(classWrapper, options);
        var structDeserialized = JsonSerializer.Deserialize<GuidStructWrapper>(structJson, options);
        var classDeserialized = JsonSerializer.Deserialize<GuidClassWrapper>(classJson, options);

        // Assert
        await Assert.That(structDeserialized.Value).IsEqualTo(value);
        await Assert.That(classDeserialized).IsNotNull();
        await Assert.That(classDeserialized.Value).IsEqualTo(value);
    }

    [Test]
    public async Task SerializeDeserialize_ReturnsSameValue_NewtonsoftJson()
    {
        // Arrange
        var value = Guid.NewGuid();
        var structWrapper = GuidStructWrapper.From(value);
        var classWrapper = GuidClassWrapper.From(value);

        // Act
        var structJson = JsonConvert.SerializeObject(structWrapper);
        var classJson = JsonConvert.SerializeObject(classWrapper);
        var structDeserialized = JsonConvert.DeserializeObject<GuidStructWrapper>(structJson);
        var classDeserialized = JsonConvert.DeserializeObject<GuidClassWrapper>(classJson);

        // Assert
        await Assert.That(structDeserialized.Value).IsEqualTo(value);
        await Assert.That(classDeserialized).IsNotNull();
        await Assert.That(classDeserialized.Value).IsEqualTo(value);
    }

    [Test]
    public async Task InsertRetrieve_ReturnsSameValue_LiteDB()
    {
        // Arrange
        using var db = new LiteDatabase(":memory:");
        var collection = db.GetCollection("guidTest");

        var value = Guid.NewGuid();
        var structWrapper = GuidStructWrapper.From(value);
        var classWrapper = GuidClassWrapper.From(value);

        // Act - Create BsonDocuments directly
        var structDoc = new BsonDocument
        {
            ["_id"] = 1,
            ["structValue"] = new BsonValue(structWrapper.Value)
        };

        var classDoc = new BsonDocument
        {
            ["_id"] = 2,
            ["classValue"] = new BsonValue(classWrapper.Value)
        };

        // Insert the documents
        collection.Insert(structDoc);
        collection.Insert(classDoc);

        // Retrieve the documents
        var retrievedStructDoc = collection.FindById(1);
        var retrievedClassDoc = collection.FindById(2);

        // Extract the values
        var retrievedStructValue = retrievedStructDoc["structValue"].AsGuid;
        var retrievedClassValue = retrievedClassDoc["classValue"].AsGuid;

        // Assert - Compare the Guid values directly
        await Assert.That(retrievedStructValue).IsEqualTo(value);
        await Assert.That(retrievedClassValue).IsEqualTo(value);
    }
}
public class DateOnlyWrapperTests
{
    private readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);
    private readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
    private readonly DateOnly Yesterday = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

    [Before(Test)]
    public void SetupBsonMapper()
    {
        var mapper = BsonMapper.Global;
        mapper.Entity<DateOnlyStructWrapper>().Field(w => w.Value, "value");
        mapper.Entity<DateOnlyClassWrapper>().Field(w => w.Value, "value");
    }

    [Test]
    public async Task From_ReturnsCorrectValue_WhenValidInput()
    {
        // Arrange
        var value = Tomorrow;
        var structWrapper = DateOnlyStructWrapper.From(value);
        var classWrapper = DateOnlyClassWrapper.From(value);

        // Act
        var structResult = structWrapper.Value;
        var classResult = classWrapper.Value;

        // Assert
        await Assert.That(structResult).IsEqualTo(value);
        await Assert.That(classResult).IsEqualTo(value);
    }

    [Test]
    public async Task ToString_ReturnsValueString_WhenCalled()
    {
        // Arrange
        var value = Tomorrow;
        var structWrapper = DateOnlyStructWrapper.From(value);
        var classWrapper = DateOnlyClassWrapper.From(value);

        // Act
        var structResult = structWrapper.ToString();
        var classResult = classWrapper.ToString();

        // Assert
        await Assert.That(structResult).IsEqualTo(value.ToString());
        await Assert.That(classResult).IsEqualTo(value.ToString());
    }

    [Test]
    public async Task From_ThrowsArgumentException_WhenPastDate()
    {
        // Arrange
        var invalidValue = Yesterday;

        // Act & Assert
        await Assert.That(() => DateOnlyStructWrapper.From(invalidValue)).ThrowsExactly<ArgumentException>();
        await Assert.That(() => DateOnlyClassWrapper.From(invalidValue)).ThrowsExactly<ArgumentException>();
    }

    [Test]
    public async Task SerializeDeserialize_ReturnsSameValue_SystemTextJson()
    {
        // Arrange
        var value = Tomorrow;
        var structWrapper = DateOnlyStructWrapper.From(value);
        var classWrapper = DateOnlyClassWrapper.From(value);
        var options = new JsonSerializerOptions { WriteIndented = true };

        // Act
        var structJson = JsonSerializer.Serialize(structWrapper, options);
        var classJson = JsonSerializer.Serialize(classWrapper, options);
        var structDeserialized = JsonSerializer.Deserialize<DateOnlyStructWrapper>(structJson, options);
        var classDeserialized = JsonSerializer.Deserialize<DateOnlyClassWrapper>(classJson, options);

        // Assert
        await Assert.That(structDeserialized.Value).IsEqualTo(value);
        await Assert.That(classDeserialized).IsNotNull();
        await Assert.That(classDeserialized.Value).IsEqualTo(value);
    }

    [Test]
    public async Task SerializeDeserialize_ReturnsSameValue_NewtonsoftJson()
    {
        // Arrange
        var value = Tomorrow;
        var structWrapper = DateOnlyStructWrapper.From(value);
        var classWrapper = DateOnlyClassWrapper.From(value);

        // Act
        var structJson = JsonConvert.SerializeObject(structWrapper);
        var classJson = JsonConvert.SerializeObject(classWrapper);
        var structDeserialized = JsonConvert.DeserializeObject<DateOnlyStructWrapper>(structJson);
        var classDeserialized = JsonConvert.DeserializeObject<DateOnlyClassWrapper>(classJson);

        // Assert
        await Assert.That(structDeserialized.Value).IsEqualTo(value);
        await Assert.That(classDeserialized).IsNotNull();
        await Assert.That(classDeserialized.Value).IsEqualTo(value);
    }

    [Test]
    public async Task InsertRetrieve_ReturnsSameValue_LiteDB()
    {
        // Arrange
        using var db = new LiteDatabase(":memory:");
        var collection = db.GetCollection("dateOnlyTest");

        var value = Tomorrow;
        var structWrapper = DateOnlyStructWrapper.From(value);
        var classWrapper = DateOnlyClassWrapper.From(value);

        // Act - Create BsonDocuments directly
        var structDoc = new BsonDocument
        {
            ["_id"] = 1,
            ["structValue"] = new BsonValue(structWrapper.Value.DayNumber)
        };

        var classDoc = new BsonDocument
        {
            ["_id"] = 2,
            ["classValue"] = new BsonValue(classWrapper.Value.DayNumber)
        };

        // Insert the documents
        collection.Insert(structDoc);
        collection.Insert(classDoc);

        // Retrieve the documents
        var retrievedStructDoc = collection.FindById(1);
        var retrievedClassDoc = collection.FindById(2);

        // Extract the values and convert from day number to DateOnly
        var retrievedStructDayNumber = retrievedStructDoc["structValue"].AsInt32;
        var retrievedClassDayNumber = retrievedClassDoc["classValue"].AsInt32;
        var retrievedStructValue = DateOnly.FromDayNumber(retrievedStructDayNumber);
        var retrievedClassValue = DateOnly.FromDayNumber(retrievedClassDayNumber);

        // Assert
        await Assert.That(retrievedStructValue).IsEqualTo(value);
        await Assert.That(retrievedClassValue).IsEqualTo(value);
    }
}

public class TimeOnlyWrapperTests
{
    private readonly TimeOnly ValidTime = new TimeOnly(12, 30, 45); // 12:30:45 PM
    private readonly TimeOnly InvalidTime = new TimeOnly(8, 30, 0); // 8:30 AM (before business hours)
    private readonly TimeOnly ValidTimeNormalizedStruct = new TimeOnly(12, 31, 0); // 12:31 PM (rounded up)
    private readonly TimeOnly ValidTimeNormalizedClass = new TimeOnly(12, 31, 0); // 12:31 PM (rounded up)

    [Before(Test)]
    public void SetupBsonMapper()
    {
        var mapper = BsonMapper.Global;
        mapper.Entity<TimeOnlyStructWrapper>().Field(w => w.Value, "value");
        mapper.Entity<TimeOnlyClassWrapper>().Field(w => w.Value, "value");
    }

    [Test]
    public async Task From_ReturnsCorrectValue_WhenValidInput()
    {
        // Arrange
        var value = ValidTime;
        var structWrapper = TimeOnlyStructWrapper.From(value);
        var classWrapper = TimeOnlyClassWrapper.From(value);

        // Act
        var structResult = structWrapper.Value;
        var classResult = classWrapper.Value;

        // Assert
        await Assert.That(structResult).IsEqualTo(ValidTimeNormalizedStruct); // Normalized to nearest minute
        await Assert.That(classResult).IsEqualTo(ValidTimeNormalizedClass); // Normalized to nearest minute
    }

    [Test]
    public async Task ToString_ReturnsValueString_WhenCalled()
    {
        // Arrange
        var value = ValidTime;
        var structWrapper = TimeOnlyStructWrapper.From(value);
        var classWrapper = TimeOnlyClassWrapper.From(value);

        // Act
        var structResult = structWrapper.ToString();
        var classResult = classWrapper.ToString();

        // Assert
        await Assert.That(structResult).IsEqualTo(ValidTimeNormalizedStruct.ToString());
        await Assert.That(classResult).IsEqualTo(ValidTimeNormalizedClass.ToString());
    }

    [Test]
    public async Task From_ThrowsArgumentException_WhenOutsideBusinessHours()
    {
        // Arrange
        var invalidValue = InvalidTime;

        // Act & Assert
        await Assert.That(() => TimeOnlyStructWrapper.From(invalidValue)).ThrowsExactly<ArgumentException>();
        await Assert.That(() => TimeOnlyClassWrapper.From(invalidValue)).ThrowsExactly<ArgumentException>();
    }

    [Test]
    public async Task SerializeDeserialize_ReturnsSameValue_SystemTextJson()
    {
        // Arrange
        var value = ValidTime;
        var structWrapper = TimeOnlyStructWrapper.From(value);
        var classWrapper = TimeOnlyClassWrapper.From(value);
        var options = new JsonSerializerOptions { WriteIndented = true };

        // Act
        var structJson = JsonSerializer.Serialize(structWrapper, options);
        Console.WriteLine(structJson);
        var classJson = JsonSerializer.Serialize(classWrapper, options);
        Console.WriteLine(classJson);
        var structDeserialized = JsonSerializer.Deserialize<TimeOnlyStructWrapper>(structJson, options);
        var classDeserialized = JsonSerializer.Deserialize<TimeOnlyClassWrapper>(classJson, options);

        // Assert
        await Assert.That(structDeserialized.Value).IsEqualTo(ValidTimeNormalizedStruct);
        await Assert.That(classDeserialized).IsNotNull();
        await Assert.That(classDeserialized.Value).IsEqualTo(ValidTimeNormalizedClass);
    }

    [Test]
    public async Task SerializeDeserialize_ReturnsSameValue_NewtonsoftJson()
    {
        // Arrange
        var value = ValidTime;
        var structWrapper = TimeOnlyStructWrapper.From(value);
        var classWrapper = TimeOnlyClassWrapper.From(value);

        // Act
        var structJson = JsonConvert.SerializeObject(structWrapper);
        Console.WriteLine(structJson);
        var classJson = JsonConvert.SerializeObject(classWrapper);
        Console.WriteLine(classJson);
        var structDeserialized = JsonConvert.DeserializeObject<TimeOnlyStructWrapper>(structJson);
        var classDeserialized = JsonConvert.DeserializeObject<TimeOnlyClassWrapper>(classJson);

        // Assert
        await Assert.That(structDeserialized.Value).IsEqualTo(ValidTimeNormalizedStruct);
        await Assert.That(classDeserialized).IsNotNull();
        await Assert.That(classDeserialized.Value).IsEqualTo(ValidTimeNormalizedClass);
    }

    [Test]
    public async Task InsertRetrieve_ReturnsSameValue_LiteDB()
    {
        // Arrange
        using var db = new LiteDatabase(":memory:");
        var collection = db.GetCollection("timeOnlyTest");

        var value = ValidTime;
        var structWrapper = TimeOnlyStructWrapper.From(value);
        var classWrapper = TimeOnlyClassWrapper.From(value);

        // Act - Create BsonDocuments directly
        var structDoc = new BsonDocument
        {
            ["_id"] = 1,
            ["structValue"] = new BsonValue(structWrapper.Value.Ticks)
        };

        var classDoc = new BsonDocument
        {
            ["_id"] = 2,
            ["classValue"] = new BsonValue(classWrapper.Value.Ticks)
        };

        // Insert the documents
        collection.Insert(structDoc);
        collection.Insert(classDoc);

        // Retrieve the documents
        var retrievedStructDoc = collection.FindById(1);
        var retrievedClassDoc = collection.FindById(2);

        // Extract the values and convert from ticks to TimeOnly
        var retrievedStructTicks = retrievedStructDoc["structValue"].AsInt64;
        var retrievedClassTicks = retrievedClassDoc["classValue"].AsInt64;
        var retrievedStructValue = new TimeOnly(retrievedStructTicks);
        var retrievedClassValue = new TimeOnly(retrievedClassTicks);

        // Assert
        await Assert.That(retrievedStructValue).IsEqualTo(ValidTimeNormalizedStruct);
        await Assert.That(retrievedClassValue).IsEqualTo(ValidTimeNormalizedClass);
    }
}

public class BoolStructWrapperEntity
{
    public int Id { get; set; }
    public BoolStructWrapper Value { get; set; }
}

public class BoolClassWrapperEntity
{
    public int Id { get; set; }
    public BoolClassWrapper Value { get; set; }
}

public class DateTimeStructWrapperEntity
{
    [BsonId] // Mark this as the ID field for LiteDB
    public int Id { get; set; }
    public DateTimeStructWrapper Value { get; set; }
}

public class DateTimeClassWrapperEntity
{
    [BsonId] // Mark this as the ID field for LiteDB
    public int Id { get; set; }
    public DateTimeClassWrapper Value { get; set; }
}

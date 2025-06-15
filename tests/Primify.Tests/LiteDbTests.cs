namespace Primify.Generator.Tests;

record MyEntity(UserId Id, Username Username, Age Age);

[Primify<Guid>]
public readonly partial record struct UserId;

[Primify<string>]
public partial struct Username;

[Primify<int>]
public partial record struct Age;

public class LiteDbTests
{
    [Fact]
    public void LiteDB_Passes_WhenSerializeDeserialize()
    {
        var id = UserId.From(Guid.CreateVersion7());
        var username = Username.From("Sue");
        var age = Age.From(19);

        var entity = new MyEntity(id, username, age);
        using var db = new LiteDatabase(":memory:");
        var col = db.GetCollection<MyEntity>("items");
        col.Insert(entity);

        var retrieved = col.FindOne(o => o.Id == id);

        Assert.Equal(entity, retrieved);
        Assert.Equal(id, retrieved.Id);
        Assert.Equal(username, retrieved.Username);
        Assert.Equal(age, retrieved.Age);

        var resultById = col.FindById(id);
        Assert.Equal(entity, resultById);
    }

    [Fact]
    public void LiteDB_Passes_WhenCastToFrom()
    {
        var id = UserId.From(Guid.CreateVersion7());
        var username = Username.From("Sue");
        var age = Age.From(19);

        var doc = new BsonDocument
        {
            ["id"] = id.Value,
            ["username"] = (string)username,
            ["age"] = (int)age
        };

        // And deserialization works too:
        UserId deserializedId = doc["id"];
        var deserializedUsername = (Username)doc["username"];
        var deserializedAge = (Age)doc["age"];

        Assert.Equal(id, deserializedId);
        Assert.Equal(username, deserializedUsername);
        Assert.Equal(age, deserializedAge);
    }
}

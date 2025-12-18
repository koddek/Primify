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

    [Fact]
    public void Query_Passes_WhenQuery()
    {
        using var db = new LiteDatabase(":memory:");
        db.GetCollection<User>("users")
            .Insert(new User(UserId.From(Guid.CreateVersion7()), Username.From("Sue"), Age.From(8)));
        db.GetCollection<User>("users")
            .Insert(new User(UserId.From(Guid.CreateVersion7()), Username.From("Storm"), Age.From(14)));
        db.GetCollection<User>("users")
            .Insert(new User(UserId.From(Guid.CreateVersion7()), Username.From("Mark"), Age.From(25)));
        db.GetCollection<User>("users")
            .Insert(new User(UserId.From(Guid.CreateVersion7()), Username.From("Kelly"), Age.From(18)));
        db.GetCollection<User>("users")
            .Insert(new User(UserId.From(Guid.CreateVersion7()), Username.From("Jon"), Age.From(34)));

        var result = db.GetCollection<User>("users")
            .Query().Where(o => ((string)o.Username).StartsWith("S"));

        Assert.Equal(2, result.Count());
    }

    record User(UserId Id, Username Username, Age Age);
}

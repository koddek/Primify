using LiteDB;
using Primify.Attributes;

namespace Primify.Demo;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var c1 = ItemNumber.From(100);
        Console.WriteLine(c1.ToString()); // ItemNumber { Value = 100 }
        Console.WriteLine(c1.Value); // 100
        Console.WriteLine();

        var s1 = ItemWeight.From(199.88);
        Console.WriteLine(s1.ToString()); // ItemWeight { Value = 199.88 }
        Console.WriteLine(s1.Value); // 199.88
        Console.WriteLine();

        var val = ItemName.From("bar");
        Console.WriteLine(val.ToString()); // ItemName { Value = bar }
        Console.WriteLine(val.Value); // bar

        // Automatic registration with BsonMapper
        using var db = new LiteDatabase(":memory:");
        var collection = db.GetCollection<ItemDocument>();

        // Insert document with wrapped types
        var itemId = ItemId.From(Guid.CreateVersion7());
        var product = new ItemDocument
        {
            Id = itemId,
            Name = ItemName.From("LiteDB Product"),
            Number = ItemNumber.From(7)
        };

        // Insert and get the ID (which is a wrapped type)
        collection.Insert(product);

        // Find by ID (most efficient lookup)
        var foundById = collection.FindById(product.Id);
        Console.WriteLine($"Found: {foundById.Name} (ID: {foundById.Id})");

        // Query by wrapped type property (works with indexes)
        var queryResult = collection.FindOne(d => d.Number == product.Number);

        // Update a document
        product.Name = ItemName.From("Updated Product");
        collection.Update(product);

        // Count documents with a specific number
        var count = collection.Count(d => d.Number != ItemNumber.Undefined);

        // Example of finding by ID string
        var productById = collection.FindById(ItemId.From(Guid.Parse(itemId.Value.ToString())));
        Console.WriteLine($"Found: {count} (ID: {productById.Id})");
    }
}

[Primify<int>]
public sealed partial record class ItemNumber
{
    public static ItemNumber Undefined { get; } = new(-1);

    private static int Normalize(int value) => value < 1 ? -1 : value;

    private static void Validate(int value)
    {
        if (value > 100)
            throw new ArgumentOutOfRangeException(nameof(value), value,
                "Value must be at minimum 1, and maximum 100.");
    }
}

[Primify<double>]
public readonly partial record struct ItemWeight
{
    public static ItemWeight Undefined { get; } = new(-1);

    private static double Normalize(double value) => value <= 0 ? -1 : value;

    private static void Validate(double value)
    {
        if (value > 1000)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be more than 0, and maximum 1000.");
    }
}

[Primify<string>]
public sealed partial record class ItemName
{
    public static ItemName Undefined { get; } = new("undefined");

    private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? "" : value;

    private static void Validate(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);

        if (value.Length > 100)
            throw new ArgumentOutOfRangeException(nameof(value), value,
                "Value length must be at min 1, and maximum 100.");
    }
}

// Simple ID wrapper type
[Primify<Guid>]
public readonly partial record struct ItemId;

// Define your document model
public class ItemDocument
{
    public ItemId Id { get; set; } = ItemId.From(Guid.Empty);
    public ItemName Name { get; set; } = ItemName.Undefined;
    public ItemNumber Number { get; set; } = ItemNumber.Undefined;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

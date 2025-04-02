# Primify

A lightweight source generator for .NET that transforms decorated record classes or readonly record structs into custom value types with built-in JSON serialization support. Primify simplifies creating strongly-typed wrappers around primitive types, with seamless integration for `System.Text.Json`, `Newtonsoft.Json`, and `LiteDB`.

## Features
- **Source Generation**: Generates boilerplate for value wrappers.
- **JSON Serialization**: Supports `System.Text.Json` and `Newtonsoft.Json`.
- **LiteDB Integration**: Registers BSON mappers for database compatibility.
- **Validation & Normalization**: Customizable via partial methods.
- **Implicit Conversions**: Convert to/from the primitive type effortlessly.
- **Targeting .NET 9**: Built for the latest .NET version.

## Installation
Install via NuGet:

```bash
dotnet add package Primify
```

## Getting Started
1. Define a value type using `PrimifyAttribute<T>`:
```csharp
using Primify.Attributes;

// Record class wrapper
[PrimifyAttribute<int>]
public sealed partial record class OrderId;

// Readonly record struct wrapper
[PrimifyAttribute<string>]
public readonly partial record struct CustomerName;
```

2. Use it in your code:
```csharp
using Primify.Attributes;
using Newtonsoft.Json;
using LiteDB;
using JsonSerializer = System.Text.Json.JsonSerializer;

// Create instances
var order = new Order
{
    Id = OrderId.From(1001),
    Customer = CustomerName.From("Alice Smith")
};

// Serialize with System.Text.Json
string json1 = JsonSerializer.Serialize(order);
Console.WriteLine(json1); // {"Id":1001,"Customer":"Alice Smith"}

// Serialize with Newtonsoft.Json
string json2 = JsonConvert.SerializeObject(order);
Console.WriteLine(json2); // {"Id":1001,"Customer":"Alice Smith"}

// Store in LiteDB
using var db = new LiteDatabase(":memory:");
var col = db.GetCollection<Order>("orders");
col.Insert(order);
var retrieved = col.FindOne(o => o.Id == OrderId.From(1001));
Console.WriteLine(retrieved.Customer); // "Alice Smith"

// Implicit conversions
int idValue = order.Id; // 1001 (to primitive)
OrderId newId = 2002;   // From primitive

public sealed class Order
{
    // Use [BsonId] attribute if your id is not named "Id"
    public OrderId Id { get; set; }
    public CustomerName Customer { get; set; }
}
```

## Advanced Usage
Customize validation and normalization:
```csharp
[PrimifyAttribute<int>]
public sealed partial record class Quantity
{
    // Validate the value
    static partial void Validate(in int value)
    {
        if (value <= 0)
            throw new ArgumentException("Quantity must be positive.");
    }

    // Normalize the value
    private static partial int Normalize(int value) => Math.Max(1, value); // Ensure at least 1
}

var qty = Quantity.From(0); // Throws due to validation
var normalizedQty = Quantity.From(-5); // Becomes 1 due to normalization
```

## Notes
- **Implicit Conversions**: Use `OrderId id = 123` or `int value = id` directly.
- **Supported Primitives**: Works with `int`, `string`, `Guid`, and other primitive types.
- **Requirements**: .NET 9.0+, C# 9.0+ (for records).

## Contributing
Contributions are welcome! Submit a pull request or open an issue on [GitHub](https://github.com/dapwell/Primify).

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgements
Inspired by [Vogen](https://github.com/SteveDunn/Vogen) and [Primitively](https://github.com/Primitively/Primitively). These library developers are Epic Level Engineers. Source generation is mind-bending.
```
```

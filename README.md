# Primify

[![Build Status](https://img.shields.io/github/actions/workflow/status/koddek/Primify/build-publish-nuget.yml?branch=main&style=for-the-badge)](https://github.com/koddek/Primify/actions/workflows/build-publish-nuget.yml)
[![NuGet Version](https://img.shields.io/badge/NuGet-1.11.1-blue?style=for-the-badge&logo=nuget)](https://github.com/koddek/Primify/pkgs/nuget/Primify)
[![License](https://img.shields.io/github/license/koddek/Primify?style=for-the-badge)](LICENSE)

**Primify** is a high-performance C# source generator that creates strongly-typed, boilerplate-free wrappers for
primitive values. It helps you eliminate "Primitive Obsession" and build more robust, expressive, and secure domain
models with zero runtime overhead.

It includes out-of-the-box serialization support for:

* **System.Text.Json**
* **Newtonsoft.Json**
* **LiteDB**

## Why Primify?

In domain-driven design, passing around primitive types like `string` for an email address or `int` for a User ID can
lead to bugs and an anemic domain model. Primify solves this by allowing you to create dedicated types for these
values instantly.

* ✅ **Type Safety:** The compiler prevents you from accidentally assigning an `EmailAddress` to a `ProductName`, even
  though both are strings.
* ✅ **Built-in Validation:** Enforce rules (e.g., max length, format) at the type level, ensuring invalid values can
  never be created.
* ✅ **Zero Boilerplate:** Define your type's rules in one place. Primify generates all the necessary boilerplate for
  equality, casting, and serialization.
* ✅ **Compile-Time Generation:** Equality, conversions, and serializer wiring are generated directly into your
  assembly — no runtime reflection or IL weaving.

## Features

- **Type-Safe Primitive Wrappers:** Generate `readonly record struct`, `record struct`, `record class`, or plain
  `class`/`struct` wrappers.
- **Built-in Validation & Normalization:** Declare optional private static hooks; invalid values can never enter
  through `From`, `TryFrom`, casts, or deserialization.
- **Non-Throwing Creation:** Every wrapper gets a generated `TryFrom(value, out result)`.
- **Out-of-the-Box Serialization:** System.Text.Json, Newtonsoft.Json (JSON + BSON), and LiteDB with built-in bridges
  for the full modern primitive set (`DateOnly`, `TimeOnly`, `TimeSpan`, `DateTimeOffset`, `Half`, ...).
- **Predefined Static Values:** Easily create common instances like `Username.Guest` or `Id.Empty`.
- **IDE Friendly:** Generated types carry `[DebuggerDisplay("{Value}")]` and compile-time diagnostics for hook
  signature mistakes.
- **High Performance:** Designed for minimal overhead, with benchmarks to prove it.

## Installation

Primify is distributed as a NuGet package.

```bash
dotnet add package Primify
```

The `Primify` package ships adapters for System.Text.Json, Newtonsoft.Json, and LiteDB, so referencing it brings
`Newtonsoft.Json`, `Newtonsoft.Json.Bson`, and `LiteDB` along as package dependencies. Wrapped types work with all
three serializers out of the box.

### Agent skill

The package carries an agent-facing skill at `skills/primify/SKILL.md` (visible in the extracted package folder,
e.g. `~/.nuget/packages/primify/<version>/skills/primify/SKILL.md`). It teaches coding agents the declaration syntax,
hook contract, generated API surface, serializer behavior, and best practices. The canonical copy lives in this repo
under [`skills/primify/SKILL.md`](skills/primify/SKILL.md).

## Getting Started

### 1. Define Your Wrapper

Create a `partial record` and decorate it with the `[Primify<T>]` attribute, where `T` is the underlying primitive type.
You can define both `record class` and `record struct` types.

```csharp
using Primify.Attributes;

// Simple ID wrapper type (Minimal). Could be struct or class.
[Primify<Guid>]
public partial struct ProductId;

// Record class example
[Primify<string>]
public sealed partial record class ProductName
{
    // Define predefined values using the private constructor (Optional)
    public static ProductName Undefined { get; } = new("undefined");
    public static ProductName Default { get; } = new("default-product");

    // Normalize is called before validation (Optional)
    private static string Normalize(string value) 
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    // Validation runs after normalization (Optional)
    private static void Validate(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Product name cannot be empty or whitespace.");
            
        if (value.Length > 100)
            throw new ArgumentException("Product name cannot exceed 100 characters.");
    }
}

// Record struct example with value type
[Primify<int>]
public readonly partial record struct ProductNumber
{
    // Predefined value for invalid/undefined state
    public static ProductNumber Undefined { get; } = new(-1);

    // Normalization ensures values are within expected range
    private static int Normalize(int value) => value < 1 ? -1 : value;

    // Validation enforces business rules
    private static void Validate(int value)
    {
        if (value > 100)
            throw new ArgumentOutOfRangeException(nameof(value), 
                "Value must be between 1 and 100.");
    }
}
```

### 2. Using Your Wrapper Types

Primify generates all the necessary boilerplate: `Value` property, private constructor, `From`/`TryFrom` factories,
conversion operators, equality members, `[DebuggerDisplay]`, serializer attributes, and LiteDB registration.

```csharp
// Creating instances using the From method
var product1 = ProductName.From("  Premium Widget  "); // Normalized to "Premium Widget"
var productNum1 = ProductNumber.From(42);

// Non-throwing creation at system boundaries (user input, config, external payloads)
if (!ProductNumber.TryFrom(userInput, out var parsed))
{
    Console.WriteLine("Invalid product number");
}

// Using predefined values
var defaultProduct = ProductName.Default;
var invalidProduct = ProductNumber.Undefined;

// Accessing the underlying value
string productName = product1.Value;
int productNumber = productNum1.Value;

// String representation
Console.WriteLine(product1); // Output: "ProductName { Value = Premium Widget }"
Console.WriteLine(productNum1); // Output: "ProductNumber { Value = 42 }"

// Equality comparison works as expected
var product2 = ProductName.From("Premium Widget");
Console.WriteLine(product1 == product2); // True (after normalization)

// Using with switch expressions
string productType = product1 switch
{
    var p when p == ProductName.Default => "Default product",
    var p when p.Value.Contains("Premium") => "Premium product",
    _ => "Standard product"
};

// Converting back to the primitive type is implicit
string nameString = product1;
int number = productNum1;

// Converting a primitive into a wrapper is explicit by design:
// it runs normalization and validation, so it can throw on invalid input.
var product3 = (ProductName)"Premium Widget";

// Using with collections
var products = new List<ProductName>
{
    ProductName.From("Product A"),
    ProductName.From("Product B"),
    ProductName.Default
};

// Serialization works out of the box
var json = JsonSerializer.Serialize(new { Product = product1, Number = productNum1 });
// {"Product":"Premium Widget","Number":42}
```

### 3. Serialization and Storage

Primify provides seamless integration with popular serialization libraries. The generated types include the necessary
converters automatically.

#### System.Text.Json

```csharp
// Serialization
var data = new { 
    Product = ProductName.From("Premium Widget"),
    Number = ProductNumber.From(42) 
};

string json = JsonSerializer.Serialize(data);
// {"Product":"Premium Widget","Number":42}

// Deserialization
var deserialized = JsonSerializer.Deserialize<YourType>(json);
```

#### Newtonsoft.Json

```csharp
// Works out of the box
string newtonJson = JsonConvert.SerializeObject(data);
var deserializedNewton = JsonConvert.DeserializeObject<YourType>(newtonJson);
```

#### LiteDB

```csharp
// Define your document model
public class ProductDocument
{
    public ProductId Id { get; set; } = ProductId.From(Guid.Empty);
    public ProductName Name { get; set; } = ProductName.Undefined;
    public ProductNumber Number { get; set; } = ProductNumber.Undefined;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Automatic registration with BsonMapper
using var db = new LiteDatabase(":memory:");
var collection = db.GetCollection<ProductDocument>();

// Insert document with wrapped types
var productId = ProductId.From(Guid.CreateVersion7());
var product = new ProductDocument
{
    Id = productId,
    Name = ProductName.From("LiteDB Product"),
    Number = ProductNumber.From(7)
};

// Insert the document
collection.Insert(product);

// Find by ID (pass the underlying primitive; wrapped types do not implicitly convert to BsonValue)
var foundById = collection.FindById(product.Id.Value);
Console.WriteLine($"Found: {foundById.Name} (ID: {foundById.Id})");

// Query by wrapped type property (works with indexes; values serialize through the registered mapping)
var queryResult = collection.FindOne(d => d.Number == product.Number);

// Update a document
product.Name = ProductName.From("Updated Product");
collection.Update(product);

// Count documents with a specific number
var count = collection.Count(d => d.Number != ProductNumber.Undefined);

// Example of finding by ID
var productById = collection.FindById(productId.Value);
Console.WriteLine($"Found by ID: {productById.Name} (Count: {count})");
```

### 4. Validation and Normalization

Primify makes it easy to enforce business rules through the `Validate` and `Normalize` methods:

```csharp
// Normalization happens automatically
var name = ProductName.From("  Extra  Spaces  ");
Console.WriteLine(name.Value); // "Extra  Spaces" (only trims leading/trailing)

// Validation prevents invalid values
try 
{
    var invalid = ProductNumber.From(101); // Throws ArgumentOutOfRangeException
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine(ex.Message);
    // "Value must be between 1 and 100. (Parameter 'value')"
}

// Non-throwing alternative
if (ProductNumber.TryFrom(101, out var result))
{
    Console.WriteLine(result.Value);
}
else
{
    Console.WriteLine("Invalid product number");
}

// Predefined values bypass validation
var undefined = ProductNumber.Undefined; // No exception thrown
```

`TryFrom` returns `false` when `Validate` throws an `ArgumentException`-derived exception
(`ArgumentNullException`, `ArgumentOutOfRangeException`, ...) and outputs the default instance otherwise.
Prefer throwing exceptions from `Validate` so both `From` and `TryFrom` behave consistently.

### 5. Working with Nullable Values

Primify works seamlessly with nullable types:

```csharp
// Nullable wrapper types
ProductName? maybeProduct = null;
if (someCondition)
    maybeProduct = ProductName.From("Dynamic Product");

// Using with Entity Framework Core
public class ProductEntity
{
    public int Id { get; set; }
    public ProductName Name { get; set; } = ProductName.Default;
    public ProductNumber? OptionalProductNumber { get; set; }
}
```

## Supported Primitive Types

Primify wraps any single-value type, and ships built-in LiteDB mappings for the full set of common primitives:

| Wrapped type | LiteDB storage | Notes |
|---|---|---|
| `string`, `bool`, `int`, `long`, `double`, `decimal`, `Guid` | native BSON type | lossless |
| `byte`, `sbyte`, `short`, `ushort`, `char` | Int32 | checked cast on read |
| `uint`, `ulong` | Int64 | follows LiteDB conventions; values beyond `long` range throw `OverflowException` |
| `float`, `Half` | Double | widened; `Half` is exact for representable values |
| `DateTime` | DateTime | normalized to UTC instants (LiteDB stores millisecond precision) |
| `TimeSpan`, `TimeOnly` | Int64 ticks | lossless |
| `DateOnly` | DateTime | stored as UTC midnight so the date survives any machine timezone |
| `DateTimeOffset` | Document `{DateTime, Offset}` | offset preserved exactly; UTC component has millisecond precision |

Other types are not blocked — register your own mapping with
`LiteDbMapping.Register<TWrapper, TValue>(mapper)` replacement logic or a custom
`BsonMapper.RegisterType<T>` call after startup.

## Advanced Usage

### LiteDB Custom Mapping

By default, Primify generates a `[ModuleInitializer]` that calls `LiteDbMapping.Register<TWrapper, TValue>()`,
which registers a BSON mapping on `BsonMapper.Global`. Registering again replaces the mapping, so you can override
any type at startup:

```csharp
// Replace the default mapping for a wrapper type.
BsonMapper.Global.RegisterType<UserId>(
    serialize: id => id.Value,
    deserialize: bson => UserId.From(bson.AsInt32)
);
```

Wrapped types do not expose implicit conversions to/from `LiteDB.BsonValue`; pass `.Value` when an API expects the
underlying primitive or a BSON value:

```csharp
var found = collection.FindById(product.Id.Value);
```

## How It Works

Primify is two assemblies acting as one:

1. **`Primify.Generators`** — an incremental Roslyn generator. At compile time it finds every `partial` type
   annotated with `[Primify<T>]`, validates the optional `Normalize`/`Validate` hook signatures, and emits a
   generated partial containing: the `Value` property, private constructor, `From`/`TryFrom`, conversion operators,
   equality members where the kind needs them, `[DebuggerDisplay]`, JSON converter attributes, and a file-scoped
   `[ModuleInitializer]`.
2. **`Primify` runtime** — the small support library your code references: `IPrimify<TSelf,TValue>` contract, the
   System.Text.Json and Newtonsoft converters, and `LiteDbMapping`, which owns every LiteDB type bridge in one
   place. The generated initializer calls `LiteDbMapping.Register<TWrapper,TValue>()` at startup; re-register any
   type afterwards to override.

Nothing runs via reflection at serialization time: converters read `.Value` directly and rebuild through the
static abstract `From`, so validation applies on deserialization exactly as at construction sites.

## Best Practices

**Keep `Normalize` a cleanup step and `Validate` an invariant check.** `Normalize` runs first; its output is what
`Validate` sees. Trim/case/lowercase/clamp in `Normalize`; enforce business rules (ranges, lengths, formats) in
`Validate`.

**Throw `ArgumentException`-derived exceptions from `Validate`.** The generated `TryFrom` catches
`ArgumentException` (and subclasses such as `ArgumentNullException`/`ArgumentOutOfRangeException`) to return `false`.
Throwing anything else escapes `TryFrom` and breaks that contract.

**Prefer wrappers with validation for domain concepts; keep bare wrappers for opaque IDs.** `[Primify<Guid>]` with no
hooks is perfect for identifiers; add hooks only when rules exist.

**Use `From` for trusted construction sites and `TryFrom` at system boundaries.** Parsing user input, reading config,
or handling external payloads: use `TryFrom` and surface your own error. Internal code paths can assume `From`.

**Remember implicit conversions only leave the wrapper.** `string s = wrapper;` works implicitly because it cannot
fail. Entering a wrapper always requires `From`, `TryFrom`, or an explicit cast — invalid data fails loudly instead of
silently flowing through an assignment.

**Predefined values (`Undefined`, `Default`) bypass validation by design.** They call the private constructor
directly, so they are safe places for sentinel states — but never construct invalid instances there casually.

**Pass `.Value` when an API expects the primitive or a BSON value.** Wrapped types intentionally do not define
implicit `BsonValue` conversions.

**One concept per type.** Don't reuse `Email` where `UserName` is meant; that's the whole point of eliminating
primitive obsession.

## Benchmarks
```csharp
// * Summary *
BenchmarkDotNet v0.15.6, macOS 26.1 (25B78) [Darwin 25.1.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.100
[Host]    : .NET 10.0.0 (10.0.0, 10.0.25.52411), Arm64 RyuJIT armv8.0-a
.NET 10.0 : .NET 10.0.0 (10.0.0, 10.0.25.52411), Arm64 RyuJIT armv8.0-a
Job=.NET 10.0  Runtime=.NET 10.0

// 1. Simple Wrapper (Zero logic)
[Primify<string>]
public readonly partial record struct SimpleUser;

// 2. Smart Wrapper (Normalization + Validation)
[Primify<string>]
public readonly partial record struct SmartUser
{
    // Normalize: Trim and Lowercase
    private static string Normalize(string value)
    {
        return value.Trim().ToLowerInvariant();
    }

    // Validate: Void method that throws if invalid
    static void Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be empty");

        if (value.Length < 3)
            throw new ArgumentException("Value must be at least 3 characters");
    }
}
```

| Method                | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------------- |-----------:|------:|-------:|----------:|------------:|
| 'Manual String Logic' | 24.5890 ns | 1.000 | 0.0127 |      80 B |        1.00 |
| SmartUser.From()      | 21.5932 ns | 0.878 | 0.0127 |      80 B |        1.00 |
| SimpleUser.From()     |  0.0000 ns | 0.000 |      - |         - |        0.00 |
| 'String == String'    |  0.0000 ns | 0.000 |      - |         - |        0.00 |
| 'Simple == Simple'    |  0.1039 ns | 0.004 |      - |         - |        0.00 |
| 'Smart == Smart'      |  1.7500 ns | 0.071 |      - |         - |        0.00 |


## Contributing

Pull requests welcome! For major changes, please open an issue first.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgements
Inspired by [Vogen](https://github.com/SteveDunn/Vogen) and [Primitively](https://github.com/Primitively/Primitively). These library developers are Epic Level Engineers. Source generation is mind-bending.

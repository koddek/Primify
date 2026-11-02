# Primify

[![Build Status](https://img.shields.io/github/actions/workflow/status/koddek/Primify/build-publish-nuget.yml?branch=main&style=for-the-badge)](https://github.com/koddek/Primify/actions/workflows/build-publish-nuget.yml)
[![NuGet Version](https://img.shields.io/badge/NuGet-1.12.0-blue?style=for-the-badge&logo=nuget)](https://github.com/koddek/Primify/pkgs/nuget/Primify)
[![License](https://img.shields.io/github/license/koddek/Primify?style=for-the-badge)](LICENSE)

**Primify** is a high-performance C# source generator that creates strongly-typed, boilerplate-free wrappers for
primitive values. It helps you eliminate "Primitive Obsession" and build more robust, expressive, and secure domain
models with zero runtime overhead.

It includes out-of-the-box serialization support for:

* **System.Text.Json** (always, it ships in the framework)
* **Newtonsoft.Json** (JSON + BSON, when your project references `Newtonsoft.Json` / `Newtonsoft.Json.Bson`)
* **LiteDB** (when your project references `LiteDB`)

The `Primify` package itself has **no dependencies**. The generator detects the serializer packages in your project
at compile time and only then emits the matching adapter code into your assembly, so wrappers serialize seamlessly
with whichever serializers you actually use.

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
- **Out-of-the-Box Serialization:** System.Text.Json always, plus Newtonsoft.Json (JSON + BSON) and LiteDB when your
  project references those packages, with built-in bridges for the full modern primitive set (`DateOnly`, `TimeOnly`,
  `TimeSpan`, `DateTimeOffset`, `Half`, ...).
- **Sentinel Values:** Call the generated private constructor from a static property to define named states such as
  `Undefined` or `Empty`. They bypass the hooks by design.
- **IDE Friendly:** Generated types carry `[DebuggerDisplay("{Value}")]` and compile-time diagnostics for hook
  signature mistakes.
- **High Performance:** Designed for minimal overhead, with benchmarks to prove it.

## Installation

Primify is distributed as a NuGet package. The current package targets .NET 10.

```bash
dotnet add package Primify
```

The `Primify` package has no package dependencies. Add the serializers you use yourself:

```bash
dotnet add package LiteDB            # optional, for LiteDB persistence
dotnet add package Newtonsoft.Json   # optional, for Json.NET support
```

When a serializer package is referenced, the generator emits its adapter into your assembly at compile time, so
wrapped types work with it out of the box. System.Text.Json needs no package because it ships in the framework.

### Agent skill

The package carries an agent-facing skill at `skills/primify/SKILL.md` (visible in the extracted package folder,
e.g. `~/.nuget/packages/primify/<version>/skills/primify/SKILL.md`). It teaches coding agents the declaration syntax,
hook contract, generated API surface, serializer behavior, and best practices. The canonical copy lives in this repo
under [`skills/primify/SKILL.md`](skills/primify/SKILL.md).

The generated and runtime API is summarized in [`docs/API.md`](docs/API.md).

## Getting Started

### 1. Define Your Wrapper

Create a `partial record` and decorate it with the `[Primify<T>]` attribute, where `T` is the underlying primitive type.
You can define `class`, `struct`, `record class`, and `record struct` wrappers. The declaration must be partial and accessible from generated namespace-level code. Generic wrappers, static, abstract, file-local, and ref-like declarations are rejected with PRIT005.

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

    // Normalize receives non-null values. It runs before validation (Optional)
    private static string Normalize(string value) => value.Trim();

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
conversion operators, equality members, `[DebuggerDisplay]`, and serializer wiring (System.Text.Json always; the
Newtonsoft.Json attribute and the LiteDB registration only when those packages are referenced).

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

// String representation. Record wrappers keep the compiler's ToString, so they print
// "TypeName { Value = ... }". Plain struct/class wrappers print the bare underlying value.
Console.WriteLine(product1);   // "ProductName { Value = Premium Widget }"  (record class)
Console.WriteLine(productNum1); // "ProductNumber { Value = 42 }"             (record struct)
Console.WriteLine(IntStruct.From(42)); // "42"                              (plain struct)

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

`DateTimeOffset` uses `{ "UtcTicks": ..., "OffsetTicks": ... }` in standard Newtonsoft JSON. BSON uses a `Value` field,
with the same tick object for `DateTimeOffset`, so the original offset is retained.

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
(`ArgumentNullException`, `ArgumentOutOfRangeException`, ...). Value-type wrappers return their default value on failure;
class wrappers return `null` and advertise that with `MaybeNullWhen(false)`. Prefer throwing exceptions from `Validate`
so both `From` and `TryFrom` behave consistently.

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

`From` rejects null reference values before calling `Normalize`. A normalizer must not return null; Primify throws
`ArgumentNullException` if it does. This keeps nullable wrapper properties separate from invalid non-null instances.

On deserialization System.Text.Json resolves JSON `null` before the converter runs, so a reference-type wrapper reads
back as `null`. A value-type wrapper has no null representation, so it throws a `JsonException` that names the wrapper
rather than the primitive. Declare the property as `Wrapper?` to represent a missing value.

## Supported Primitive Types

When your project references LiteDB, Primify generates built-in mappings for the common primitives below. Values without
a built-in LiteDB bridge are still valid wrapper types (PRIT007 warns about them), but their persistence mapping must be
registered explicitly.

| Wrapped type | LiteDB storage | Notes |
|---|---|---|
| `string`, `bool`, `int`, `long`, `double`, `decimal`, `Guid` | native BSON type | lossless |
| `byte`, `sbyte`, `short`, `ushort`, `char` | Int32 | checked cast on read |
| `uint`, `ulong` | Int64 | follows LiteDB conventions; values beyond `long` range throw `OverflowException` |
| `float`, `Half` | Double | widened; `Half` is exact for representable values |
| `DateTime` | DateTime | normalized to UTC instants (LiteDB stores millisecond precision) |
| `TimeSpan`, `TimeOnly` | Int64 ticks | lossless |
| `DateOnly` | DateTime | stored as UTC midnight so the date survives any machine timezone |
| `DateTimeOffset` | Document `{UtcTicks, OffsetTicks}` | instant and offset both exact to the tick |

For Newtonsoft.Json, `DateTimeOffset` uses `{ "UtcTicks": ..., "OffsetTicks": ... }` in standard JSON and inside the BSON
`Value` field. This avoids local-time conversion and preserves the original offset. LiteDB stores the same tick pair
in its own document. Other supported values use their normal JSON representation.

Other types are not blocked — register your own mapping with a custom
`BsonMapper.RegisterType<TWrapper>` call after startup. A stored document that does not match the shape a mapping
writes raises a `FormatException` naming the wrapper, instead of failing deeper inside LiteDB.

## Advanced Usage

### LiteDB Custom Mapping

When your project references LiteDB, Primify generates a `[ModuleInitializer]` that registers every wrapper with a
built-in bridge on `BsonMapper.Global` through the generated `Primify.Converters.LiteDbRegistration` class. Wrappers
without a built-in bridge are skipped (PRIT007 warns at compile time), so assembly startup does not crash. Registering
a mapping again replaces the previous mapping, so you can override any type at startup:

```csharp
// Replace the default mapping for a wrapper type.
BsonMapper.Global.RegisterType<UserId>(
    serialize: id => id.Value,
    deserialize: bson => UserId.From(bson.AsInt32)
);
```

For a custom mapper instead of `BsonMapper.Global`, register all wrappers in one call:

```csharp
var mapper = new BsonMapper();
Primify.Converters.LiteDbRegistration.Register(mapper);
using var db = new LiteDatabase("orders.db", mapper);
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
   equality members where the kind needs them, `[DebuggerDisplay]`, and the System.Text.Json converter attribute
   (plus the Newtonsoft.Json attribute when that package is referenced). When LiteDB is referenced, the generator also
   emits a shared `LiteDbRegistration` source with a `[ModuleInitializer]`.
2. **`Primify` runtime** — the small support library your code references:

   | Namespace | Contents |
   |---|---|
   | `Primify` | `IPrimify<TSelf, TValue>`, the one contract every generated wrapper implements |
   | `Primify.Attributes` | `PrimifyAttribute<TPrimitive>` |
   | `Primify.Converters` | `SystemTextJsonConverter<,>` |

   The package has no serializer dependencies. When your project references Newtonsoft.Json, the generator emits a
   `NewtonsoftJsonConverter<,>` into your assembly and attaches it via `[JsonConverter]`; when it references LiteDB, it
   emits a `LiteDbRegistration` whose `[ModuleInitializer]` registers every wrapper on `BsonMapper.Global` (call
   `LiteDbRegistration.Register(mapper)` for a custom mapper). Re-register any type afterwards to override.

Nothing runs via reflection at serialization time: converters read `.Value` directly and rebuild through the
static abstract `From`, so validation applies on deserialization exactly as at construction sites.

Generic helpers constrain on the root contract with a single import:

```csharp
using Primify;

static TSelf Remap<TSelf, TValue>(TSelf wrapper, Func<TValue, TValue> transform)
    where TSelf : IPrimify<TSelf, TValue>
    => TSelf.From(transform(wrapper.Value));
```

### Upgrading to 2.0

Two breaking changes, both in the package's dependency footprint:

- **The package no longer brings `LiteDB`, `Newtonsoft.Json`, or `Newtonsoft.Json.Bson` along.** Add the packages you
  use with `dotnet add package ...`. Support for them is now generated into your assembly at compile time, only when
  the package is referenced, so behavior is unchanged once the package is present.
- **`LiteDbMapping` and `NewtonsoftJsonConverter` are gone from `Primify.Converters`.** LiteDB registration now happens
  through the generated `Primify.Converters.LiteDbRegistration` (`Register(BsonMapper)`); Newtonsoft.Json works through
  the generated `[JsonConverter]` attribute with no action needed. Wrappers of types without a built-in LiteDB bridge
  now also produce a PRIT007 warning at compile time.

Stored LiteDB documents and JSON payloads keep the same shapes as 1.x, so existing data remains readable.

### Upgrading from 1.11.x

Two changes affect existing code:

- **`IPrimify<,>` moved** from `Primify.Converters` to the root `Primify` namespace. Replace
  `using Primify.Converters;` with `using Primify;` in files that constrain on the interface. Attribute usage and
  serializer adapters are unaffected.
- **LiteDB `DateTimeOffset` documents changed shape** from `{DateTime, Offset}` to `{UtcTicks, OffsetTicks}`. The old
  shape silently truncated the instant to milliseconds; the new one is exact to the tick. Reading an old document now
  throws a `FormatException` that says so, rather than returning a truncated value. Re-index or migrate those
  documents if you store `DateTimeOffset` wrappers in LiteDB.

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
BenchmarkDotNet v0.15.8, macOS 26.1 (25B78) [Darwin 25.1.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.401
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

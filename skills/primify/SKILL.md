---
name: primify
description: Use when creating or consuming Primify wrapper types — adding [Primify<T>] attributes to eliminate primitive obsession, defining Normalize/Validate hooks, calling From/TryFrom or conversions, wiring JSON (System.Text.Json / Newtonsoft) or LiteDB persistence for wrapped types, fixing PRIT00xx generator diagnostics, or choosing between wrapper kinds (class/struct/record).
---

# Primify

Primify is a C# source generator that completes `[Primify<T>]`-annotated partial types into strongly-typed
primitive wrappers. It exists to kill primitive obsession: an `EmailAddress` is never accidentally assigned from a
`UserName`, and invalid values cannot exist because every entry point validates.

## Architecture overview

Two assemblies act as one:

1. **`Primify.Generators`** (compile-time only, shipped under `analyzers/dotnet/cs`) — an incremental Roslyn
   generator. It finds `[Primify<T>]`-annotated partial types, validates hook signatures (errors PRIT002/PRIT003 on
   mismatch), and emits a generated partial with: `Value` property, private constructor, `From`/`TryFrom`, explicit
   and implicit conversion operators, equality members for non-record kinds, `[DebuggerDisplay("{Value}")]`,
   JSON converter attributes, and a file-scoped `[ModuleInitializer]`.
2. **`Primify` runtime** — `IPrimify<TSelf,TValue>` contract (reflection-free serializer plumbing),
   `SystemTextJsonConverter<,>`, `NewtonsoftJsonConverter<,>`, and `LiteDbMapping` (single owner of every LiteDB
   type bridge; the generated initializer calls `LiteDbMapping.Register<TWrapper,TValue>()` at startup).

Primary API entry points in user code: the attribute (`Primify.Attributes.PrimifyAttribute<T>`), generated statics
(`From` / `TryFrom`), conversion operators, `.Value`, and — only when overriding persistence —
`LiteDbMapping.Register`.

## Install

```bash
dotnet add package Primify
```

One package: runtime types (`Primify.Converters.*`) plus the generator shipped under `analyzers/dotnet/cs`.
Transitive dependencies: Newtonsoft.Json, Newtonsoft.Json.Bson, LiteDB.

## Declaring a wrapper

Annotate a **partial** type; all four shapes are supported:

```csharp
using Primify.Attributes;

[Primify<Guid>]
public partial struct OrderId;                       // plain struct

[Primify<string>]
public readonly partial record struct Sku;           // record struct

[Primify<int>]
public partial class Quantity;                       // plain class

[Primify<string>]
public sealed partial record class EmailAddress      // record class
{
    public static EmailAddress Empty { get; } = new("");   // predefined values bypass validation

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static void Validate(string value)
    {
        if (!value.Contains('@'))
            throw new ArgumentException("Not a valid email address.", nameof(value));
    }
}
```

### Hook contract

| Hook | Signature | Runs | Purpose |
|---|---|---|---|
| `Normalize` | `private static T Normalize(T value)` | first | cleanup: trim, lowercase, clamp |
| `Validate` | `private static void Validate(T value)` | second | invariants: throw `ArgumentException`-derived |

Both optional. Wrong visibility/signature = compile error **PRIT002** (Normalize) / **PRIT003** (Validate). Other
diagnostics: **PRIT001** undeterminable wrapped type, **PRIT004** containing type must be partial.

`Validate` MUST throw `ArgumentException`-derived exceptions (`ArgumentNullException`,
`ArgumentOutOfRangeException`, ...). The generated `TryFrom` relies on this to return `false`; anything else escapes.

## Generated API surface

For wrapper `W` around `T`:

| Member | Notes |
|---|---|
| `T Value { get; }` | underlying value |
| `static W From(T value)` | normalize → validate → construct; throws on invalid |
| `static bool TryFrom(T value, out W result)` | non-throwing; `result` is `default(W)` on failure |
| `(W)t` explicit cast | same pipeline as `From`; throwing cast |
| `(T)w` implicit cast | leaves the wrapper; cannot fail |
| `ToString()` | records print `Name { Value = x }`; plain kinds print raw value |
| equality | `Equals`, `==`/`!=`, `GetHashCode`; records compiler-generated, others hand-rolled ordinal for strings |
| `[DebuggerDisplay("{Value}")]` | attached automatically |

Entering a wrapper is **always explicit** (`From`/`TryFrom`/cast) so invalid data fails loudly. Leaving is implicit.

## Serialization

- **System.Text.Json** — payload carries the bare primitive; reads rebuild through `From` (invalid payloads throw the
  validation exception). Nullable wrapper properties round-trip `null` cleanly.
- **Newtonsoft.Json** — same for JSON; BSON writers use a `{ "Value": ... }` document shape.
- **LiteDB** — generated `[ModuleInitializer]` calls `LiteDbMapping.Register<W, T>()` on `BsonMapper.Global`.
  Re-register to override. Built-in bridges:

  | Wrapped type | Stored as | Caveats |
  |---|---|---|
  | `string bool int long double decimal Guid` | native | lossless |
  | `byte sbyte short ushort char` | Int32 | checked on read |
  | `uint ulong` | Int64 | overflow beyond long range throws |
  | `float Half` | Double | widened |
  | `DateTime` | DateTime | UTC instants, ms precision |
  | `TimeSpan TimeOnly` | Int64 ticks | lossless |
  | `DateOnly` | DateTime | UTC midnight; timezone-safe |
  | `DateTimeOffset` | `{DateTime, Offset}` doc | offset exact; ms precision |

  Wrapped types have **no** implicit `BsonValue` conversion — pass `.Value`: `collection.FindById(orderId.Value)`.

## Best practices

1. Bare wrappers (`[Primify<Guid>]`, no hooks) for opaque IDs; add hooks only where rules exist.
2. `Normalize` never produces values `Validate` rejects — that ordering is guaranteed and tested.
3. `TryFrom` at system boundaries (user input, config, external payloads); `From` inside trusted code.
4. Predefined statics call the private constructor directly and bypass validation — reserve them for real sentinels.
5. One domain concept per type; do not reuse `Email` where `UserName` is meant.
6. Prefer `readonly record struct` for small values; use record/class when identity semantics or inheritance matter.

## Anti-patterns to avoid

```csharp
// WRONG: assuming implicit primitive -> wrapper conversion exists
EmailAddress email = "a@b.com";          // compile error; entering a wrapper is explicit
var email2 = (EmailAddress)"  A@B.COM "; // correct: explicit cast runs normalize + validate

// WRONG: throwing non-ArgumentException types from Validate
private static void Validate(string v)
{
    if (v.Length > 5) throw new InvalidOperationException("too long"); // escapes TryFrom!
}

// WRONG: relying on default(struct) as a valid instance
Sku sku = default;                       // bypasses validation entirely (Value is null)
if (sku == Sku.Undefined) { }            // sentinels must be built with the private ctor

// WRONG: expecting implicit BsonValue conversions on wrappers
collection.FindById(orderId);            // compile error since the bridge rewrite
collection.FindById(orderId.Value);      // correct

// WRONG: hand-rolling serialization plumbing the generator already emits
public class EmailJsonConverter : JsonConverter<EmailAddress> { ... }   // unnecessary
[JsonConverter(typeof(EmailJsonConverter))]                             // already attached

// WRONG: duplicating validation outside the wrapper
if (!userInput.Contains('@')) throw ...; var email = EmailAddress.From(userInput); // rules live in Validate
```

Also avoid: declaring hooks without `private static`, forgetting `partial` on the type or any containing type
(PRIT004), and wrapping types without a LiteDB bridge without re-registering a custom mapping.

## Verification habits (for agents editing wrappers)

After adding or changing a wrapper, compile and run the consumer's tests. If authoring tests for a Primify-consuming
project, cover at minimum: `From` happy path, validation failure (exception type), `TryFrom` failure returning
default, one serializer round-trip actually used by the project, and equality/hash usage if stored in collections.

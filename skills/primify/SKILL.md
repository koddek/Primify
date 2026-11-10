---
name: primify
description: Use when creating or consuming Primify wrapper types, adding [Primify<T>] attributes, defining Normalize/Validate hooks, using From/TryFrom or conversions, wiring JSON or LiteDB persistence, or fixing PRIT00xx diagnostics.
---

# Primify

Primify is a .NET 10 incremental source generator. It completes `[Primify<T>]` annotated partial types with
strongly-typed wrapper members, validation, equality, conversion operators, and serializer adapters.

## Package layout

The `Primify` NuGet package has no dependencies and contains:

- `Primify.Generators.dll` under `analyzers/dotnet/cs`.
- `IPrimify<,>` in `Primify`, the contract every generated wrapper implements.
- `PrimifyAttribute<T>` in `Primify.Attributes`.
- The System.Text.Json adapter `SystemTextJsonConverter<,>` in `Primify.Converters`.
- This skill at `skills/primify/SKILL.md`.

Newtonsoft.Json and LiteDB adapters are not in the package. When the consuming project references those packages, the
generator emits the adapters into the consumer assembly at compile time, so support appears automatically.

Generic helpers constrain on the root contract, so a single import is enough:

```csharp
using Primify;

static TSelf Remap<TSelf, TValue>(TSelf wrapper, Func<TValue, TValue> transform)
    where TSelf : IPrimify<TSelf, TValue>
    => TSelf.From(transform(wrapper.Value));
```

## Declare a wrapper

Use a non-generic, accessible `class`, `struct`, `record class`, or `record struct`. Every declaration and containing
type must be partial. Static, abstract, file-local, ref-like, generic, and inaccessible nested declarations are rejected.

```csharp
using Primify.Attributes;

[Primify<Guid>]
public partial struct OrderId;

[Primify<string>]
public sealed partial record class UserName
{
    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static void Validate(string value)
    {
        if (value.Length == 0)
            throw new ArgumentException("Name is required.", nameof(value));
    }
}
```

Do not declare `Value`, `From`, `TryFrom`, conversion operators, or generated equality members yourself. A conflict
reports PRIT006. `PrimifyAttribute<T>` is `AllowMultiple = false`, so a second attributed partial is a `CS0579`
compile error from the compiler.

## Hook contract

Both hooks are optional:

| Hook | Signature | Order |
|---|---|---|
| `Normalize` | `private static T Normalize(T value)` | first |
| `Validate` | `private static void Validate(T value)` | second |

Hooks must be synchronous, non-generic, private, static, and use one by-value parameter. `ref`, `out`, generic, and
async hooks report PRIT002 or PRIT003. Invalid hooks stop source generation.

`From` rejects null reference values before `Normalize`. A reference normalizer must return a non-null value. `Validate`
must throw an `ArgumentException`-derived exception to reject a value. `TryFrom` returns `false` whenever `From` throws,
whatever the exception type.

## Generated API

For a wrapper `W` around `T`:

| Member | Contract |
|---|---|
| `T Value { get; }` | Underlying normalized value. |
| `static W From(T value)` | Runs `Normalize`, null checking, and `Validate`. |
| `static bool TryFrom(T value, out W result)` | Returns `false` when `From` throws, whatever the exception type. Class results are null on failure. |
| `explicit operator W(T value)` | Same validation pipeline as `From`. |
| `implicit operator T(W value)` | Reads the underlying primitive; a null wrapper reference throws `ArgumentNullException`. |
| `ToString` and equality | Record types use compiler-generated members; plain types use generated members. |

Plain `class`/`struct` wrappers print the bare underlying value from `ToString`. Record wrappers keep the compiler's
`ToString`, which prints `TypeName { Value = ... }`.

Use `From` at trusted construction sites and `TryFrom` at input, configuration, and external-data boundaries. Do not use
`default` as proof of validity. Sentinel values call the private constructor and bypass hooks by design.

## Serialization

### System.Text.Json

The generated converter writes the bare primitive and rebuilds through `From`. Nullable wrapper properties can be null.
System.Text.Json resolves JSON `null` before the converter runs, so a reference-type wrapper reads back as `null`. A
value-type wrapper throws a `JsonException` naming the wrapper, because it cannot represent a missing value; declare the
property as `Wrapper?`.

### Newtonsoft.Json

Generated only when the project references `Newtonsoft.Json` (the BSON branches also need `Newtonsoft.Json.Bson`): an
internal `NewtonsoftJsonConverter<W, T>` emitted into the consumer assembly and attached to every wrapper via
`[JsonConverter]`. Standard JSON writes the bare primitive. `DateTimeOffset` uses `{ "UtcTicks": ..., "OffsetTicks": ... }`
because Json.NET otherwise converts ISO values to the local offset before the wrapper converter runs. BSON wraps the
value in `{ "Value": ... }`; a `DateTimeOffset` uses the tick object inside `Value`. Malformed BSON objects are rejected.

### LiteDB

Generated only when the project references `LiteDB`: an internal `LiteDbRegistration` class whose `[ModuleInitializer]` registers
every wrapper with a built-in bridge on `BsonMapper.Global`; call `LiteDbRegistration.Register(mapper)` from your own
assembly for a custom mapper. Values without a built-in bridge are skipped (PRIT007 warns at compile time), so they do not
crash assembly startup. Register an unsupported wrapper explicitly with `BsonMapper.RegisterType<W>(...)` after startup.

Built-in storage mappings include native BSON values, checked integer conversions, `DateTime` stored as a UTC instant
(`Unspecified` is treated as UTC, `Local` is converted), `TimeSpan` and `TimeOnly` ticks, `DateOnly` UTC midnight, and a
`{ UtcTicks, OffsetTicks }` document for `DateTimeOffset`, which keeps both the instant and the offset exact to the tick.
A wrapper around `Nullable<T>` of a supported type stores a `{ "Value": bson }` document whose `Value` is BSON null when
unset. A stored document that does not match the shape a mapping writes raises a `FormatException` naming the wrapper —
unexpected BSON type, out-of-range number, or a malformed `DateTimeOffset` document — never a raw LiteDB failure.

## Diagnostics

| ID | Meaning |
|---|---|
| PRIT001 | Invalid wrapped type. |
| PRIT002 | Invalid `Normalize` signature or overload. |
| PRIT003 | Invalid `Validate` signature or overload. |
| PRIT004 | A containing type is not partial. |
| PRIT005 | Unsupported declaration shape or inaccessible nested wrapper. |
| PRIT006 | Reserved generated member conflict. |
| PRIT007 | LiteDB is referenced but the wrapped type has no built-in LiteDB bridge (warning). |

## Agent verification

After changing a wrapper, build the consumer and run its tests. Cover `From`, validation failure, `TryFrom` failure,
the serializer used by the consumer, equality or hash use when stored in collections, and any custom LiteDB mapping.
For generator changes, run the full test executable and include compile-backed cases for generated source.

For a `DateTimeOffset` wrapper, test with a sub-millisecond value. The LiteDB bridge is tick-based, so a value that
would have been truncated by a BSON `DateTime` must still compare equal after a round trip.

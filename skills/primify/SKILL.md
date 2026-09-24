---
name: primify
description: Use when creating or consuming Primify wrapper types, adding [Primify<T>] attributes, defining Normalize/Validate hooks, using From/TryFrom or conversions, wiring JSON or LiteDB persistence, or fixing PRIT00xx diagnostics.
---

# Primify

Primify is a .NET 10 incremental source generator. It completes `[Primify<T>]` annotated partial types with
strongly-typed wrapper members, validation, equality, conversion operators, and serializer adapters.

## Package layout

The `Primify` NuGet package contains:

- `Primify.Generators.dll` and `Flowgen.dll` under `analyzers/dotnet/cs`.
- Runtime types under `Primify.Attributes` and `Primify.Converters`.
- `Newtonsoft.Json`, `Newtonsoft.Json.Bson`, and `LiteDB` dependencies.
- This skill at `skills/primify/SKILL.md`.

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
reports PRIT006.

## Hook contract

Both hooks are optional:

| Hook | Signature | Order |
|---|---|---|
| `Normalize` | `private static T Normalize(T value)` | first |
| `Validate` | `private static void Validate(T value)` | second |

Hooks must be synchronous, non-generic, private, static, and use one by-value parameter. `ref`, `out`, generic, and
async hooks report PRIT002 or PRIT003. Invalid hooks stop source generation.

`From` rejects null reference values before `Normalize`. A reference normalizer must return a non-null value. `Validate`
must throw an `ArgumentException`-derived exception to reject a value. `TryFrom` catches that exception family and
returns `false`; other exceptions escape.

## Generated API

For a wrapper `W` around `T`:

| Member | Contract |
|---|---|
| `T Value { get; }` | Underlying normalized value. |
| `static W From(T value)` | Runs `Normalize`, null checking, and `Validate`. |
| `static bool TryFrom(T value, out W result)` | Returns `false` on `ArgumentException`. Class results are null on failure. |
| `explicit operator W(T value)` | Same validation pipeline as `From`. |
| `implicit operator T(W value)` | Leaves the wrapper and cannot fail. |
| `ToString` and equality | Record types use compiler-generated members; plain types use generated members. |

Use `From` at trusted construction sites and `TryFrom` at input, configuration, and external-data boundaries. Do not use
`default` as proof of validity. Predefined sentinel values call the private constructor and bypass hooks by design.

## Serialization

### System.Text.Json

The generated converter writes the bare primitive and rebuilds through `From`. Nullable wrapper properties can be null.

### Newtonsoft.Json

Standard JSON writes the bare primitive. `DateTimeOffset` uses `{ "UtcTicks": ..., "OffsetTicks": ... }` because
Json.NET otherwise converts ISO values to the local offset before the wrapper converter runs. BSON wraps the value in
`{ "Value": ... }`; a `DateTimeOffset` uses the tick object inside `Value`. Malformed BSON objects are rejected.

### LiteDB

The generated module initializer calls `LiteDbMapping.TryRegister<W, T>()`. Built-in values register on
`BsonMapper.Global`. Values without a built-in bridge are skipped, so they do not crash assembly startup. Register an
unsupported wrapper explicitly with `BsonMapper.RegisterType<W>(...)` or a custom `LiteDbMapping.Register` path.

Built-in storage mappings include native BSON values, checked integer conversions, `DateTime` UTC values, `TimeSpan`
and `TimeOnly` ticks, `DateOnly` UTC midnight, and a `{ DateTime, Offset }` document for `DateTimeOffset`.

## Diagnostics

| ID | Meaning |
|---|---|
| PRIT001 | Invalid wrapped type. |
| PRIT002 | Invalid `Normalize` signature or overload. |
| PRIT003 | Invalid `Validate` signature or overload. |
| PRIT004 | A containing type is not partial. |
| PRIT005 | Unsupported declaration shape or inaccessible nested wrapper. |
| PRIT006 | Reserved generated member conflict. |

## Agent verification

After changing a wrapper, build the consumer and run its tests. Cover `From`, validation failure, `TryFrom` failure,
the serializer used by the consumer, equality or hash use when stored in collections, and any custom LiteDB mapping.
For generator changes, run the full test executable and include compile-backed cases for generated source.

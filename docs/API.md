# Primify API

Primify targets .NET 10. The package contains the runtime support library and the incremental source generator.

## Declaration

Apply `PrimifyAttribute<T>` to a partial class or struct. Supported forms are plain classes, plain structs, record
classes, and record structs. The declaration must be non-generic, non-static, non-abstract, non-file-local, and not
ref-like. Nested declarations must be accessible from generated namespace-level code.

```csharp
using Primify.Attributes;

[Primify<string>]
public sealed partial record class UserName;
```

The generator owns these member names: `Value`, `From`, `TryFrom`, conversion operators, and, for non-record wrappers,
`ToString`, equality members, and equality operators. A user declaration of one of these members reports PRIT006.

## Construction

| Member | Behavior |
|---|---|
| `TPrimitive Value { get; }` | Returns the normalized value. |
| `static W From(TPrimitive value)` | Applies `Normalize`, checks for a null result, applies `Validate`, and constructs the wrapper. |
| `static bool TryFrom(TPrimitive value, out W result)` | Returns `false` when an `ArgumentException` is thrown. Value wrappers return `default`; class wrappers return `null` and use `MaybeNullWhen(false)`. |
| `explicit operator W(TPrimitive value)` | Runs the same pipeline as `From`. |
| `implicit operator TPrimitive(W value)` | Returns `Value` and cannot fail. |

`From` rejects null reference values before calling `Normalize`. `Normalize` must return a non-null reference value.

## Hooks

Hooks are optional, private, static, synchronous, non-generic methods with one by-value parameter of the wrapped type.

```csharp
[Primify<string>]
public partial record class UserName
{
    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static void Validate(string value)
    {
        if (value.Length == 0)
            throw new ArgumentException("Name is required.", nameof(value));
    }
}
```

`Normalize` runs before `Validate`. `Validate` must throw an `ArgumentException`-derived exception to reject a value.
Other exceptions escape `TryFrom`.

## Diagnostics

| ID | Meaning |
|---|---|
| PRIT001 | The wrapped type could not be determined. |
| PRIT002 | `Normalize` has an invalid signature or overload. |
| PRIT003 | `Validate` has an invalid signature or overload. |
| PRIT004 | A containing type is not partial. |
| PRIT005 | The declaration is unsupported, including generic, static, abstract, file-local, ref-like, or inaccessible wrappers. |
| PRIT006 | A user declaration conflicts with a generated member. |

## Serialization

### System.Text.Json

The generated converter writes the underlying primitive and rebuilds through `From`. JSON `null` is handled by
System.Text.Json before the converter reads a value.

### Newtonsoft.Json

Standard JSON writes the underlying primitive. `DateTimeOffset` is represented as an object with `UtcTicks` and
`OffsetTicks` so its original offset survives Json.NET date tokenization. BSON wraps values in `{ "Value": ... }`; a
`DateTimeOffset` value uses the same tick object inside `Value`. Malformed objects are rejected.

### LiteDB

The generated module initializer calls `LiteDbMapping.TryRegister<TWrapper, TValue>()`. Built-in mappings are
registered on `BsonMapper.Global`. Values without a built-in bridge are skipped rather than failing assembly startup.
Register those values explicitly with `BsonMapper.RegisterType<TWrapper>(...)` or `LiteDbMapping.Register` when a
custom bridge is available.

Supported built-in values include strings, booleans, signed and unsigned integers, `Guid`, `DateTime`, `DateTimeOffset`,
`TimeSpan`, `TimeOnly`, `DateOnly`, `decimal`, `float`, and `Half`. LiteDB stores time values according to the mappings
documented in the README.

## Equality

Record wrappers use compiler-generated equality. Plain wrappers use ordinal string equality or
`EqualityComparer<T>.Default` for values whose `==` operator does not provide the expected semantics. In particular,
floating-point wrappers treat `NaN` as equal to another `NaN` value.

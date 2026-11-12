# Primify API

Primify multi-targets .NET 10 and .NET 11. The package contains the runtime support library and the incremental source generator, and has
no package dependencies. Serializer adapters for Newtonsoft.Json and LiteDB are generated into the consumer assembly at
compile time, only when that package is referenced; System.Text.Json ships in the framework and is always wired up.

## Namespaces

| Namespace | Contents |
|---|---|
| `Primify` | `IPrimify<TSelf, TValue>`, the contract every generated wrapper implements |
| `Primify.Attributes` | `PrimifyAttribute<TPrimitive>` |
| `Primify.Converters` | `SystemTextJsonConverter<TWrapper, TValue>` (Newtonsoft.Json and LiteDB adapters are generated into the consumer, not shipped here) |

## Declaration

Apply `PrimifyAttribute<T>` to a partial class or struct. Supported forms are plain classes, plain structs, record
classes, and record structs. The declaration must be non-generic, non-static, non-abstract, non-file-local, and not
ref-like. Nested declarations must be accessible from generated namespace-level code, and every containing type must
be partial.

```csharp
using Primify.Attributes;

[Primify<string>]
public sealed partial record class UserName;
```

The generator owns these member names: `Value`, `From`, `TryFrom`, conversion operators, and, for non-record wrappers,
`ToString`, equality members, and equality operators. A user declaration of one of these members reports PRIT006.

`AllowMultiple` is `false`, so two attributed partial declarations of the same type are a `CS0579` compile error
reported by the compiler, not by Primify.

## Construction

| Member | Behavior |
|---|---|
| `TPrimitive Value { get; }` | Returns the normalized value. |
| `static W From(TPrimitive value)` | Applies `Normalize`, checks for a null result, applies `Validate`, and constructs the wrapper. |
| `static bool TryFrom(TPrimitive value, out W result)` | Returns `false` when `From` throws, whatever the exception type. Value wrappers return `default`; class wrappers return `null` and use `MaybeNullWhen(false)`. |
| `explicit operator W(TPrimitive value)` | Runs the same pipeline as `From`. |
| `implicit operator TPrimitive(W value)` | Returns `Value`; a null wrapper reference is rejected with `ArgumentNullException`. |

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
`TryFrom` returns `false` whenever `From` throws, whatever the exception type.

## Diagnostics

Each diagnostic carries a `helpLinkUri` pointing at this section, so the IDE's help action lands here.

| ID | Meaning |
|---|---|
| PRIT001 | The wrapped type could not be determined. |
| PRIT002 | `Normalize` has an invalid signature or overload. |
| PRIT003 | `Validate` has an invalid signature or overload. |
| PRIT004 | A containing type is not partial. |
| PRIT005 | The declaration is unsupported, including generic, static, abstract, file-local, ref-like, or inaccessible wrappers. |
| PRIT006 | A user declaration conflicts with a generated member. |
| PRIT007 | LiteDB is referenced but the wrapped type has no built-in LiteDB bridge (warning). |

## Serialization

### System.Text.Json

The generated converter writes the underlying primitive and rebuilds through `From`. System.Text.Json resolves JSON
`null` before the converter runs, so a reference-type wrapper reads back as `null`. A value-type wrapper has no null
representation and throws a `JsonException` naming the wrapper; declare the property as `Wrapper?` instead.

The reflection-based serializer (default options) is the zero-configuration path. The System.Text.Json source generator
cannot see attributes attached by another source generator, so the `[JsonConverter]` on a generated wrapper is inert when
a `JsonSerializerContext` serializes it. For source-gen and Native AOT applications, register the converters on the
context: `[JsonSourceGenerationOptions(Converters = [typeof(SystemTextJsonConverter<W, T>), ...])]`. Without it, a wrapper
serializes as `{}` and deserializes as `default` silently.

`Read`/`Write` use `JsonTypeInfo` overloads, so the adapter is trimming- and AOT-clean (the runtime assembly is marked
`IsAotCompatible`/`IsTrimmable`). Under AOT, consumers must use a context as described above.

### Newtonsoft.Json

Emitted only when the consumer references `Newtonsoft.Json` (plus `Newtonsoft.Json.Bson` for the BSON branches): a
generated internal `NewtonsoftJsonConverter<TWrapper, TValue>` in `Primify.Converters`, attached to every wrapper via
`[JsonConverter]`. Standard JSON writes the underlying primitive. `DateTimeOffset` is represented as an object with
`UtcTicks` and `OffsetTicks` so its original offset survives Json.NET date tokenization. BSON wraps values in
`{ "Value": ... }`; a `DateTimeOffset` value uses the same tick object inside `Value`. Malformed objects are rejected.

### LiteDB

Emitted only when the consumer references `LiteDB`: one shared source per assembly with an internal `LiteDbRegistration` class.
Its `[ModuleInitializer]` registers every wrapper with a built-in bridge on `BsonMapper.Global`; call
`LiteDbRegistration.Register(mapper)` for a custom mapper. Values without a built-in bridge are skipped (PRIT007 warns
at compile time) rather than failing assembly startup. Register those values explicitly with
`BsonMapper.RegisterType<TWrapper>(...)` when a custom bridge is available.

Supported built-in values include strings, booleans, signed and unsigned integers, `Guid`, `DateTime`, `DateTimeOffset`,
`TimeSpan`, `TimeOnly`, `DateOnly`, `decimal`, `float`, and `Half`, plus `Nullable<T>` of those (stored as a
`{ "Value": bson }` document; `Value` is BSON null when the wrapper holds no value). A stored document that does not
match the shape a mapping writes raises a `FormatException` naming the wrapper, whatever the failure mode: unexpected
BSON type, out-of-range numeric, or malformed `DateTimeOffset` document. An `Unspecified`-kind `DateTime` is treated as
UTC on write; a `Local` DateTime is converted. LiteDB stores time values according to the mappings documented in the README.

## Equality

Record wrappers use compiler-generated equality. Plain wrappers compare with ordinal string equality for `string`, with
the type's own `==` when that operator is reflexive, and with `EqualityComparer<T>.Default` otherwise. The strategy is
chosen from the wrapped symbol, not from how it was spelled, so an alias or `global::` qualification does not change
it. Floating-point wrappers therefore treat `NaN` as equal to another `NaN` value.

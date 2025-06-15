# Primify

[![NuGet Version](https://img.shields.io/github/v/release/koddek/Primify?include_prereleases&label=NuGet)](https://github.com/koddek/Primify/packages)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## CURRENTLY THIS IS JUST A TOY PROJECT TO SEE IF I CAN DO IT

**Primify** is a Roslyn source generator that creates type-safe wrappers for primitive values with built-in serialization support for:
- System.Text.Json
- Newtonsoft.Json
- LiteDB

## Features

✅ Strongly-typed wrappers for primitives  
✅ Automatic serialization support  
✅ Custom validation & normalization  
✅ Predefined value support  
✅ Zero runtime dependencies  

## Installation

Add the NuGet package from GitHub Packages:

```bash
dotnet add package Primify --version 1.1.1 <--- NOT YET LIVE
```

## Usage

### 1. Define Your Wrapper

```csharp
[Primify<string>]
public partial record class Username
{
    [PredefinedValue("")]
    public static partial Username Empty { get; }

    [PredefinedValue("guest")] 
    public static partial Username Guest { get; }

    // Custom validation
    static partial void Validate(string value)
    {
        if (value.Length > 20)
            throw new ArgumentException("Username too long");
    }

    // Automatic normalization
    private static partial string Normalize(string value) 
        => value.Trim().ToLowerInvariant();
}
```

### 2. Use Like Regular Values

```csharp
// Explicit conversion to wrapper
Username user = (Username)"alice123";

// Explicit conversion to raw
string raw = (string)user;

// Predefined values
if (user == Username.Guest)
{
    // ...
}
```

### 3. Serialization Just Works

```csharp
// System.Text.Json
var json = JsonSerializer.Serialize(user); // "alice123"
var back = JsonSerializer.Deserialize<Username>(json);

// Newtonsoft.Json
var newtonJson = JsonConvert.SerializeObject(user); // "alice123"
var backNewton = JsonConvert.DeserializeObject<Username>(newtonJson);

// LiteDB (automatic registration)
using var db = new LiteDatabase(":memory:");
var col = db.GetCollection<User>();
col.Insert(new User { Username = user });
```

## Supported Types

Primify works with:
- All .NET primitives (`int`, `string`, `bool`, etc.)
- `Guid`
- `DateTime`
- `TimeSpan`
- `DateTimeOffset`
- `DateOnly`
- `TimeOnly`

## Advanced Features

### Custom JSON Converters

```csharp
[Primify<int>]
[JsonConverter(typeof(CustomConverter))] // Add your own
public partial record class UserId { }
```

### LiteDB Custom Mapping

```csharp
// Auto-registered by default
// Manually register if needed:
BsonMapper.Global.RegisterType<UserId>(
    v => v.Value,
    bson => UserId.From(bson.AsInt32)
);
```

## Benchmarks
```csharp
// * Summary *

BenchmarkDotNet v0.15.1, macOS Sequoia 15.5 (24F74) [Darwin 24.5.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK 9.0.300
  [Host]     : .NET 9.0.5 (9.0.525.21509), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 9.0.5 (9.0.525.21509), Arm64 RyuJIT AdvSIMD

// This wrapper was used
[Primify<string>]
public readonly partial record struct Username;
```
| Method              | Mean      | Error    | StdDev    | Median    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |----------:|---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Serialize_Raw       | 117.55 ns | 5.104 ns | 14.561 ns | 110.75 ns |  1.01 |    0.17 | 0.0076 |      48 B |        1.00 |
| Serialize_Wrapper   | 151.54 ns | 3.096 ns |  6.393 ns | 150.04 ns |  1.31 |    0.15 | 0.0076 |      48 B |        1.00 |
| Deserialize_Raw     |  91.54 ns | 1.644 ns |  2.304 ns |  91.33 ns |  0.79 |    0.09 | 0.0063 |      40 B |        0.83 |
| Deserialize_Wrapper | 256.73 ns | 4.824 ns |  4.512 ns | 257.59 ns |  2.21 |    0.24 | 0.0062 |      40 B |        0.83 |

## Contributing

Pull requests welcome! For major changes, please open an issue first.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgements
Inspired by [Vogen](https://github.com/SteveDunn/Vogen) and [Primitively](https://github.com/Primitively/Primitively). These library developers are Epic Level Engineers. Source generation is mind-bending.

using Primify.Attributes;

namespace Primify.Generator.Tests.Types;

// Basic structs
[Primify<int>]
public partial struct IntStruct;

[Primify<string>]
public partial struct StringStruct;

// Record structs
[Primify<int>]
public partial record struct IntRecordStruct;

[Primify<string>]
public partial record struct StringRecordStruct;

// Readonly structs
[Primify<int>]
public readonly partial struct ReadonlyIntStruct;

[Primify<int>]
public readonly partial record struct ReadonlyIntRecordStruct;

// Structs with default property (predefined property)
[Primify<int>]
public partial struct IntStructWithDefaultProperty
{
    public static IntStructWithDefaultProperty Empty => new(-1);
}

[Primify<int>]
public partial record struct IntRecordStructWithDefaultProperty
{
    public static IntRecordStructWithDefaultProperty Empty => new(-1);
}

// Structs with normalization
[Primify<int>]
public partial struct IntStructWithNormalization
{
    private static int Normalize(int value) => value < 1 ? -1 : value;
}

[Primify<int>]
public partial record struct IntRecordStructWithNormalization
{
    private static int Normalize(int value) => value < 1 ? -1 : value;
}

// Structs with validation
[Primify<int>]
public partial struct IntStructWithValidation
{
    private static void Validate(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
}

[Primify<int>]
public partial record struct IntRecordStructWithValidation
{
    private static void Validate(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
}

// Structs with all features (normalization + validation + default property)
[Primify<int>]
public partial struct IntStructWithAllFeatures
{
    private static int Normalize(int value) => value < 0 ? 0 : value;
    private static void Validate(int value)
    {
        if (value > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
    
    public static IntStructWithAllFeatures Empty => new(0);
}

[Primify<int>]
public partial record struct IntRecordStructWithAllFeatures
{
    private static int Normalize(int value) => value < 0 ? 0 : value;
    private static void Validate(int value)
    {
        if (value > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
    
    public static IntRecordStructWithAllFeatures Empty => new(0);
}

// Equality test types
[Primify<int>]
public partial struct StructId;

[Primify<int>]
public readonly partial struct ReadonlyStructId;

[Primify<int>]
public partial record struct RecordStructId;

[Primify<int>]
public readonly partial record struct ReadonlyRecordStructId;

using Primify.Attributes;

namespace Primify.Generator.Tests.Types;

// Basic classes
[Primify<int>]
public partial class IntClass;

[Primify<string>]
public partial class StringClass;

// Record classes
[Primify<int>]
public partial record class IntRecordClass;

[Primify<string>]
public partial record class StringRecordClass;

// Sealed classes
[Primify<int>]
public sealed partial class SealedIntClass;

[Primify<int>]
public sealed partial record class SealedIntRecordClass;

// Classes with default property (predefined property)
[Primify<int>]
public partial class IntClassWithDefaultProperty
{
    public static IntClassWithDefaultProperty Empty => new(-1);
}

[Primify<int>]
public partial record class IntRecordClassWithDefaultProperty
{
    public static IntRecordClassWithDefaultProperty Empty => new(-1);
}

// Classes with normalization
[Primify<int>]
public partial class IntClassWithNormalization
{
    private static int Normalize(int value) => value < 1 ? -1 : value;
}

[Primify<int>]
public partial record class IntRecordClassWithNormalization
{
    private static int Normalize(int value) => value < 1 ? -1 : value;
}

// Classes with validation
[Primify<int>]
public partial class IntClassWithValidation
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
public partial record class IntRecordClassWithValidation
{
    private static void Validate(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
}

// Classes with all features (normalization + validation + default property)
[Primify<int>]
public partial class IntClassWithAllFeatures
{
    private static int Normalize(int value) => value < 0 ? 0 : value;
    private static void Validate(int value)
    {
        if (value > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
    
    public static IntClassWithAllFeatures Empty => new(0);
}

[Primify<int>]
public partial record class IntRecordClassWithAllFeatures
{
    private static int Normalize(int value) => value < 0 ? 0 : value;
    private static void Validate(int value)
    {
        if (value > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
    
    public static IntRecordClassWithAllFeatures Empty => new(0);
}

// Class with normalization + validation to prove normalize runs first
[Primify<string>]
public partial class StringClassWithNormalizeAndValidate
{
    private static string Normalize(string value) => value.Trim();

    private static void Validate(string value)
    {
        if (value.Length > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
}

// Equality test types
[Primify<int>]
public partial class ClassId;

[Primify<int>]
public sealed partial class SealedClassId;

[Primify<int>]
public partial record class RecordClassId;

[Primify<int>]
public sealed partial record class SealedRecordClassId;

namespace Primify.Benchmarks;

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

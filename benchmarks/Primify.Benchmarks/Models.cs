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

    // Validate: return true for valid values (Primify uses a bool guard)
    private static bool Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (value.Length < 3)
        {
            return false;
        }

        return true;
    }
}

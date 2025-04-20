namespace Primify.Benchmarks;

[Primify<string>]
public readonly partial record struct Username;

[Primify<string>]
public partial record class Username2;
/*{
    [PredefinedValue("")]
    public static partial Username Empty { get; }

    [PredefinedValue("guest")]
    public static partial Username Undefined { get; }

    // Custom validation logic
    static partial void Validate(string value)
    {
        if (value.Contains("@") || string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Invalid username address");
    }

    private static partial string Normalize(string value)
    {
        return value.ToLower();
    }
}*/

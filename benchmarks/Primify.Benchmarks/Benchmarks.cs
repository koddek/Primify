using BenchmarkDotNet.Jobs;

namespace Primify.Benchmarks;

[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
[HideColumns("Error", "StdDev", "Median", "RatioSD")]
public class UsernameBenchmarks
{
    // We use a value that WILL PASS validation to measure the "Happy Path" speed.
    // Throwing exceptions in a benchmark loop ruins the metrics.
    private const string DirtyValue = "  JohnDoe  ";
    private const string CleanValue = "johndoe";

    private SimpleUser _simple1;
    private SimpleUser _simple2;

    private SmartUser _smart1;
    private SmartUser _smart2;

    [GlobalSetup]
    public void Setup()
    {
        _simple1 = SimpleUser.From(CleanValue);
        _simple2 = SimpleUser.From(CleanValue);

        _smart1 = SmartUser.From(DirtyValue);
        _smart2 = SmartUser.From(DirtyValue);
    }

    // ----------------------------------------------------
    // 1. CREATION (The main overhead test)
    // ----------------------------------------------------

    [Benchmark(Baseline = true, Description = "Manual String Logic")]
    public string Create_Manual()
    {
        // 1. Normalize
        string val = DirtyValue.Trim().ToLowerInvariant();

        // 2. Validate (Mirroring the wrapper logic)
        if (string.IsNullOrWhiteSpace(val))
            throw new ArgumentException("Value cannot be empty");
        if (val.Length < 3)
            throw new ArgumentException("Value must be at least 3 characters");

        // 3. Return
        return val;
    }

    [Benchmark(Description = "SmartUser.From()")]
    public SmartUser Create_Smart()
    {
        // This calls the exact same logic as above, but inside the generated .From()
        return SmartUser.From(DirtyValue);
    }

    [Benchmark(Description = "SimpleUser.From()")]
    public SimpleUser Create_Simple()
    {
        // Baseline for "Zero Overhead" wrapper
        return SimpleUser.From(CleanValue);
    }

    // ----------------------------------------------------
    // 2. EQUALITY
    // ----------------------------------------------------

    [Benchmark(Description = "String == String")]
    public bool Equals_String()
    {
        return CleanValue == CleanValue;
    }

    [Benchmark(Description = "Simple == Simple")]
    public bool Equals_Simple()
    {
        return _simple1 == _simple2;
    }

    [Benchmark(Description = "Smart == Smart")]
    public bool Equals_Smart()
    {
        return _smart1 == _smart2;
    }
}

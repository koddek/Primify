namespace Primify.Benchmarks;

[MemoryDiagnoser]
public class PrimifyBenchmarks
{
    // Serialization benchmarks
    [Benchmark(Baseline = true)]
    public string Serialize_Raw() => JsonSerializer.Serialize("testUser123");

    [Benchmark]
    public string Serialize_Wrapper() => JsonSerializer.Serialize(Username.From("testUser123"));

    // Deserialization benchmarks (no baseline)
    [Benchmark]
    public string Deserialize_Raw() => JsonSerializer.Deserialize<string>("\"testUser123\"")!;

    [Benchmark]
    public Username Deserialize_Wrapper() => JsonSerializer.Deserialize<Username>("\"testUser123\"")!;
}

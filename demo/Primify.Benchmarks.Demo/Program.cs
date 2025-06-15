namespace Primify.Benchmarks.Demo;

using Primify.Attributes;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Text.Json;

[MemoryDiagnoser]
public class PrimifyBenchmarks
{
    private const string RawValue = "testUser";
    private static readonly Username Wrapper = Username.From(RawValue);
    private string _rawJson;
    private string _wrapperJson;

    [GlobalSetup]
    public void Setup()
    {
        // Prepare valid JSON inputs for deserialization
        _rawJson = JsonSerializer.Serialize(RawValue); // Produces "\"testUser\""
        _wrapperJson = JsonSerializer.Serialize(Wrapper); // Produces "testUser" (assuming Primify<string> serializes as string)
    }

    [Benchmark(Baseline = true)]
    public string Serialize_Raw()
    {
        return JsonSerializer.Serialize(RawValue);
    }

    [Benchmark]
    public string Serialize_Wrapper()
    {
        return JsonSerializer.Serialize(Wrapper);
    }

    [Benchmark]
    public string Deserialize_Raw()
    {
        return JsonSerializer.Deserialize<string>(_rawJson);
    }

    [Benchmark]
    public Username Deserialize_Wrapper()
    {
        return JsonSerializer.Deserialize<Username>(_wrapperJson);
    }
}

class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<PrimifyBenchmarks>();
    }
}

[Primify<string>]
public readonly partial record struct Username;

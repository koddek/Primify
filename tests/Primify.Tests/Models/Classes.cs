namespace Primify.Generator.Tests.Models;

public class Classes
{
}

[Primify<int>]
public sealed partial class Class3;

[Primify<int>]
public sealed partial record class Class4;

[Primify<double>]
public readonly partial struct Struct3;

[Primify<double>]
public readonly partial record struct Struct4;

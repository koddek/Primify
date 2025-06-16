namespace Primify.Generators;

public static class Names
{
    internal const string AttributesNamespace = "Primify.Attributes";

    internal const string PrimifyAttName = "Primify";
    internal const string PrimifyAttNameWithPostfix = $"{PrimifyAttName}Attribute";
    internal const string PrimifyAttGeneratedFilename = $"{PrimifyAttNameWithPostfix}.g.cs";
    internal const string PrimifyAttFullName = $"{AttributesNamespace}.{PrimifyAttNameWithPostfix}";
    internal const string PrimifyAttFullNameT = $"{PrimifyAttFullName}`1";

    internal const string DefinedInstanceAttName = "Primify";
    internal const string DefinedInstanceAttNameWithPostfix = $"{DefinedInstanceAttName}Attribute";
    internal const string DefinedInstanceAttFullName = $"{AttributesNamespace}.{DefinedInstanceAttNameWithPostfix}";
}

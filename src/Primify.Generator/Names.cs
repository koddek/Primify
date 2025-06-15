namespace Primify.Generator;

public static class Names
{
    internal const string AttributesNamespace = "Primify.Attributes";

    internal const string PrimifyAttName = "Primify";
    internal const string PrimifyAttNameWithPostfix = $"{PrimifyAttName}Attribute";
    internal const string PrimifyAttFullName = $"{AttributesNamespace}.{PrimifyAttNameWithPostfix}";

    internal const string DefinedInstanceAttName = "Primify";
    internal const string DefinedInstanceAttNameWithPostfix = $"{DefinedInstanceAttName}Attribute";
    internal const string DefinedInstanceAttFullName = $"{AttributesNamespace}.{DefinedInstanceAttNameWithPostfix}";
}

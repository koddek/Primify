namespace Primify.Generator;

internal static class ValueWrapperDiagnostics
{
    public static readonly DiagnosticDescriptor ErrorAttributeNotFound = new(
        id: "VWG001",
        title: "Attribute Not Found",
        messageFormat: "Could not find the required attribute '{0}' in the compilation",
        category: "ValueWrapperGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ErrorNotPartial = new(
        id: "VWG002",
        title: "Not Partial",
        messageFormat: "The type '{0}' decorated with '{1}' must be declared with the 'partial' keyword",
        category: "ValueWrapperGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ErrorNotRecordClassOrStruct = new(
        id: "VWG003",
        title: "Not Record Class or Record Struct",
        messageFormat: "The type '{0}' decorated with '{1}' must be a record class or readonly record struct",
        category: "ValueWrapperGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ErrorNotReadonlyStruct = new(
        id: "VWG004",
        title: "Not Readonly Struct",
        messageFormat: "The struct type '{0}' decorated with '{1}' must be declared with the 'readonly' modifier",
        category: "ValueWrapperGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ErrorInvalidAttributeUsage = new(
        id: "VWG005",
        title: "Invalid Attribute Usage",
        messageFormat: "{0} on type '{1}' has invalid type arguments or configuration: {2}",
        category: "ValueWrapperGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InfoGeneratedCode = new(
        id: "VWG006",
        title: "Generated Code",
        messageFormat: "Generated code for '{0}'",
        category: "ValueWrapperGenerator",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InfoPartialMethodAvailable = new(
        id: "VWG007",
        title: "Partial Method Available",
        messageFormat:
        "A partial method '{0}' for {1} is available for type '{2}'. You may implement it in your own code to customize behavior.",
        category: "ValueWrapperGenerator",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ErrorPredefinedFieldRequirements = new(        id: "VWG008",
        title: "Invalid Predefined Field",
        messageFormat: "Field '{0}' decorated with PredefinedValueAttribute must be static, readonly, and private",
        category: "ValueWrapperGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ErrorPredefinedPropertyNotPartial = new(
        id: "VWG009",
        title: "Invalid Predefined Property",
        messageFormat:
        "Property '{0}' decorated with PredefinedValueAttribute must be declared with the 'partial' keyword",
        category: "ValueWrapperGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}

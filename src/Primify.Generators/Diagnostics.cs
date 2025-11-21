namespace Primify.Generators;

public static class Diagnostics
{
    public static readonly DiagnosticDescriptor TypeMustBePartial = new(
        id: "PRIT002",
        title: "Type Must Be Partial",
        messageFormat: "Type '{0}' must be declared as partial to be used with Primify.",
        category: "TypeValidation",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TypeMustHaveAttributes = new(
        id: "PRIT003",
        title: "No Attributes Found",
        messageFormat: "Type '{0}' does not have any attributes.",
        category: "TypeValidation",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    // Usage Hints
    public static readonly DiagnosticDescriptor ImplementValidate = new(
        id: "PRIU002",
        title: "Validation Available",
        messageFormat:
        "Custom validation can be added by implementing 'private static partial void Validate({0} value)'.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ImplementNormalize = new(
        id: "PRIU003",
        title: "Normalization Available",
        messageFormat:
        "Value sanitization can be added by implementing 'private static partial {0} Normalize({0} value)'.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor AddFactory = new(
        id: "PRIU004",
        title: "Factory Properties Available",
        messageFormat:
        "Predefined instances can be added via the private constructor: 'public static {0} Default {{ get; }} = new {0}(...);'",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    // Hidden diagnostic to silently ignore false positives
    public static readonly DiagnosticDescriptor Ignore = new(
        id: "PRIT000",
        title: "Ignored",
        messageFormat: "Ignored",
        category: "Gen",
        defaultSeverity: DiagnosticSeverity.Hidden,
        isEnabledByDefault: true);
}

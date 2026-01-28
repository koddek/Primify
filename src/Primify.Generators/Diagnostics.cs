namespace Primify.Generators;

public static class Diagnostics
{
    public static readonly DiagnosticDescriptor InvalidType = new(
        id: "PRIT001",
        title: "Invalid Primify Type",
        messageFormat: "Could not determine wrapped type for '{0}'.",
        category: "TypeValidation",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ImplementValidate = new(
        id: "PRIU002",
        title: "Validation Available",
        messageFormat: "Custom validation can be added by implementing 'private static partial void Validate({0} value)'",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ImplementNormalize = new(
        id: "PRIU003",
        title: "Normalization Available",
        messageFormat: "Value sanitization can be added by implementing 'private static partial {0} Normalize({0} value)'.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor AddFactory = new(
        id: "PRIU004",
        title: "Factory Properties Available",
        messageFormat: "Predefined instances can be added via the private constructor: 'public static {0} Default {{ get; }} = new {0}(...);'",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);
}
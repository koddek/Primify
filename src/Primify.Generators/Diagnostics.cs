namespace Primify.Generators;

internal static class Diagnostics
{
    public static readonly DiagnosticDescriptor InvalidType = new(
        id: "PRIT001",
        title: "Invalid Primify Type",
        messageFormat: "Could not determine wrapped type for '{0}'",
        category: "TypeValidation",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidNormalizeSignature = new(
        id: "PRIT002",
        title: "Invalid Normalize Signature",
        messageFormat: "'{0}' must declare 'private static {1} Normalize({1} value)' for normalization to run",
        category: "TypeValidation",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidValidateSignature = new(
        id: "PRIT003",
        title: "Invalid Validate Signature",
        messageFormat: "'{0}' must declare 'private static void Validate({1} value)' for validation to run",
        category: "TypeValidation",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ContainingTypeMustBePartial = new(
        id: "PRIT004",
        title: "Containing Type Must Be Partial",
        messageFormat: "Nested Primify type '{0}' requires every containing type to be declared partial",
        category: "TypeValidation",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnsupportedTypeDeclaration = new(
        id: "PRIT005",
        title: "Unsupported Primify Type Declaration",
        messageFormat: "Primify cannot generate '{0}': {1}",
        category: "TypeValidation",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ReservedMemberConflict = new(
        id: "PRIT006",
        title: "Reserved Primify Member",
        messageFormat: "Primify wrapper '{0}' already declares reserved member '{1}'",
        category: "TypeValidation",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}

using Microsoft.CodeAnalysis;

namespace Primify.Generators;

/// <summary>
/// Contains all diagnostic descriptors used by the Primify generator.
/// </summary>
public static class Diagnostics
{
    // Diagnostic categories
    private const string WrapperGenerator = "Primify.Generators";
    private const string TypeValidation = "TypeValidation";
    private const string Usage = "Usage";
    private const string Validation = "Validation";
    private const string CodeGeneration = "CodeGeneration";

    #region Type Validation Diagnostics (PRITxxx)

    /// <summary>
    /// Type must be a class, struct, or record.
    /// </summary>
    public static readonly DiagnosticDescriptor TypeMustBeClassStructOrRecord = new(
        id: "PRIT001",
        title: "Unsupported Type Kind",
        messageFormat: "Type '{0}' is not a supported type. Only classes, structs, and records are supported.",
        category: TypeValidation,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Type must be declared as partial.
    /// </summary>
    public static readonly DiagnosticDescriptor TypeMustBePartial = new(
        id: "PRIT002",
        title: "Type Must Be Partial",
        messageFormat: "Type '{0}' must be declared as partial to be used with Primify.",
        category: TypeValidation,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Type must have attributes to be processed.
    /// </summary>
    public static readonly DiagnosticDescriptor TypeMustHaveAttributes = new(
        id: "PRIT003",
        title: "No Attributes Found",
        messageFormat: "Type '{0}' does not have any attributes. Add a [Primify] attribute to generate wrapper code.",
        category: TypeValidation,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// Type must have a Primify attribute.
    /// </summary>
    public static readonly DiagnosticDescriptor TypeMustHaveWrapAttribute = new(
        id: "PRIT004",
        title: "Missing Primify Attribute",
        messageFormat: "Type '{0}' does not have a [Primify] attribute. Add [Primify] to generate wrapper code.",
        category: TypeValidation,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    #endregion

    #region Usage Guidelines (PRIUxxx)


    /// <summary>
    /// Suggests adding predefined instance properties.
    /// </summary>
    public static readonly DiagnosticDescriptor PredefinedInstanceSuggestion = new(
        id: "PRIU001",
        title: "Predefined Instance Suggestion",
        messageFormat: "Consider adding predefined instance properties for better usability. Example:\n[PredefinedInstance(-1)] public static partial {0} Empty {{ get; }}",
        category: Usage,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    /// <summary>
    /// Informs about available partial methods for customization.
    /// </summary>
    public static readonly DiagnosticDescriptor PartialMethodAvailable = new(
        id: "PRIU002",
        title: "Partial Method Available",
        messageFormat: "Partial method '{0}' is available for type '{1}'. Implement it to customize behavior.",
        category: Usage,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    /// <summary>
    /// Provides implementation guidance for the type.
    /// </summary>
    public static readonly DiagnosticDescriptor ImplementationGuidance = new(
        id: "PRIU003",
        title: "Implementation Guidance",
        messageFormat: "For type '{0}':" +
                     "• Add predefined values using getter only static properties assigned with the private constructor." +
                     "• Implement 'private static partial bool Validate({0} value)' for custom validation" +
                     "• Implement 'private static partial {0} Normalize({0} value)' for value normalization",
        category: Usage,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    #endregion

    #region Validation (PRIVxxx)


    /// <summary>
    /// Validate method has incorrect signature.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidValidateSignature = new(
        id: "PRIV001",
        title: "Invalid Validate Method Signature",
        messageFormat: "The Validate method for type '{0}' must be 'private static partial bool Validate({0} value)'",
        category: Validation,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Normalize method has incorrect signature.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidNormalizeSignature = new(
        id: "PRIV002",
        title: "Invalid Normalize Method Signature",
        messageFormat: "The Normalize method for type '{0}' must be 'private static partial {0} Normalize({0} value)'",
        category: Validation,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    #endregion

    #region Code Generation (PRIGxxx)


    /// <summary>
    /// Required attribute was not found in the compilation.
    /// </summary>
    public static readonly DiagnosticDescriptor AttributeNotFound = new(
        id: "PRIG001",
        title: "Required Attribute Not Found",
        messageFormat: "Could not find required attribute '{0}' in the compilation",
        category: CodeGeneration,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Code generation completed successfully.
    /// </summary>
    public static readonly DiagnosticDescriptor CodeGeneratedSuccessfully = new(
        id: "PRIG002",
        title: "Code Generation Complete",
        messageFormat: "Successfully generated code for type: {0}",
        category: CodeGeneration,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a diagnostic for an unsupported type kind.
    /// </summary>
    public static Diagnostic CreateUnsupportedTypeKind(Location location, string typeName) =>
        Diagnostic.Create(TypeMustBeClassStructOrRecord, location, typeName);

    /// <summary>
    /// Creates a diagnostic for a missing partial modifier.
    /// </summary>
    public static Diagnostic CreateMissingPartialModifier(Location location, string typeName) =>
        Diagnostic.Create(TypeMustBePartial, location, typeName);

    /// <summary>
    /// Creates a diagnostic for a type missing attributes.
    /// </summary>
    public static Diagnostic CreateNoAttributesFound(Location location, string typeName) =>
        Diagnostic.Create(TypeMustHaveAttributes, location, typeName);

    /// <summary>
    /// Creates a diagnostic for a missing Primify attribute.
    /// </summary>
    public static Diagnostic CreateMissingWrapAttribute(Location location, string typeName) =>
        Diagnostic.Create(TypeMustHaveWrapAttribute, location, typeName);

    #endregion
}

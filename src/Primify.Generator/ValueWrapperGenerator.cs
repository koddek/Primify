﻿#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.ComponentModel;

#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static partial class IsExternalInit
    {
    }
}
#endif

namespace Primify.Generator
{
    [Generator]
    public class ValueWrapperGenerator : IIncrementalGenerator
    {
        // --- Diagnostic Descriptors ---
        private static readonly DiagnosticDescriptor ErrorAttributeNotFound = new(
            id: "VWG001",
            title: "Attribute Not Found",
            messageFormat: "Could not find the required attribute '{0}' in the compilation",
            category: "ValueWrapperGenerator",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor ErrorNotPartial = new(
            id: "VWG002",
            title: "Not Partial",
            messageFormat: "The type '{0}' decorated with '{1}' must be declared with the 'partial' keyword",
            category: "ValueWrapperGenerator",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor ErrorNotRecordClassOrStruct = new(
            id: "VWG003",
            title: "Not Record Class or Record Struct",
            messageFormat: "The type '{0}' decorated with '{1}' must be a record class or readonly record struct",
            category: "ValueWrapperGenerator",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor ErrorNotReadonlyStruct = new(
            id: "VWG004",
            title: "Not Readonly Struct",
            messageFormat: "The struct type '{0}' decorated with '{1}' must be declared with the 'readonly' modifier",
            category: "ValueWrapperGenerator",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor ErrorInvalidAttributeUsage = new(
            id: "VWG005",
            title: "Invalid Attribute Usage",
            messageFormat: "{0} on type '{1}' has invalid type arguments or configuration: {2}",
            category: "ValueWrapperGenerator",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor InfoGeneratedCode = new(
            id: "VWG006",
            title: "Generated Code",
            messageFormat: "Generated implementation for value wrapper type '{0}'",
            category: "ValueWrapperGenerator",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor InfoPartialMethodAvailable = new(
            id: "VWG007",
            title: "Partial Method Available",
            messageFormat: "Partial method '{0}' can be implemented to add custom {1} logic for '{2}'",
            category: "ValueWrapperGenerator",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor ErrorPredefinedFieldRequirements = new(
            id: "VWG008",
            title: "Invalid Predefined Field",
            messageFormat: "Field '{0}' decorated with PredefinedValueAttribute must be static, readonly, and private",
            category: "ValueWrapperGenerator",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        // --- Attribute Name ---
        private const string AttributeFullName = "Primify.Attributes.PrimifyAttribute`1";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register syntax provider to collect TypeDeclarationSyntax nodes with attributes
            var typeDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => IsCandidateTypeDeclaration(s),
                    transform: static (ctx, _) => (TypeDeclarationSyntax)ctx.Node)
                .Where(static t => t is not null);

            // Combine with compilation
            IncrementalValueProvider<(Compilation, ImmutableArray<TypeDeclarationSyntax>)> compilationAndTypes =
                context.CompilationProvider.Combine(typeDeclarations.Collect());

            // Register source output action
            context.RegisterSourceOutput(compilationAndTypes,
                (spc, source) => Execute(source.Item1, source.Item2, spc));
        }

        private static bool IsCandidateTypeDeclaration(SyntaxNode node)
        {
            // Look for classes, structs, record classes and record structs with attributes
            if (node is not (TypeDeclarationSyntax typeDeclaration and
                (ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax)))
            {
                return false;
            }

            // Check if it has attributes
            if (typeDeclaration.AttributeLists.Count == 0)
            {
                return false;
            }

            // Look for attributes that likely match our pattern (more efficient first pass)
            foreach (var attrList in typeDeclaration.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    var attrName = attr.Name.ToString();
                    if (attrName.Contains("Primify") ||
                        attrName.EndsWith("PrimifyAttribute"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void Execute(Compilation compilation, ImmutableArray<TypeDeclarationSyntax> types,
            SourceProductionContext context
        )
        {
            if (types.IsDefaultOrEmpty)
            {
                return;
            }

            var attributeSymbol = compilation.GetTypeByMetadataName(AttributeFullName);
            if (attributeSymbol == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(ErrorAttributeNotFound, Location.None, AttributeFullName));
                return;
            }

            var typesToProcess = new List<WrapperTypeInfo>();

            Phase1CollectAndValidateTypes(compilation, types, context, attributeSymbol, typesToProcess);

            Phase2GenerateCode(context, typesToProcess, compilation);
        }

        private static void Phase1CollectAndValidateTypes(Compilation compilation,
            ImmutableArray<TypeDeclarationSyntax> types,
            SourceProductionContext context, INamedTypeSymbol attributeSymbol, List<WrapperTypeInfo> typesToProcess
        )
        {
            foreach (var typeDeclSyntax in types)
            {
                var semanticModel = compilation.GetSemanticModel(typeDeclSyntax.SyntaxTree);
                if (semanticModel.GetDeclaredSymbol(typeDeclSyntax) is not INamedTypeSymbol typeSymbol) continue;

                var attrData = typeSymbol.GetAttributes().FirstOrDefault(ad =>
                    ad.AttributeClass != null && (
                        ad.AttributeClass.Name.Contains("Primify") ||
                        (ad.AttributeClass.OriginalDefinition != null &&
                         SymbolEqualityComparer.Default.Equals(ad.AttributeClass.OriginalDefinition, attributeSymbol))
                    ));

                if (attrData == null) continue;

                if (attrData.AttributeClass?.TypeArguments.Length != 1)
                {
                    context.ReportDiagnostic(Diagnostic.Create(ErrorInvalidAttributeUsage,
                        typeDeclSyntax.Identifier.GetLocation(), attributeSymbol.Name, typeSymbol.Name,
                        "Requires exactly one type argument (the primitive type)"));
                    continue;
                }

                var primitiveTypeSymbol = attrData.AttributeClass.TypeArguments[0];

                // Special handling for known system types that need support
                bool isPrimitiveOrSupportedType = false;

                // Check if it's an intrinsic type first
                if (primitiveTypeSymbol.SpecialType != SpecialType.None)
                {
                    isPrimitiveOrSupportedType = true;
                }
                else
                {
                    // Check additional supported types by qualified name
                    string fullQualifiedTypeName =
                        primitiveTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    isPrimitiveOrSupportedType = fullQualifiedTypeName switch
                    {
                        "global::System.Guid" => true,
                        "global::System.DateTime" => true,
                        "global::System.TimeSpan" => true,
                        "global::System.DateTimeOffset" => true,
                        "global::System.DateOnly" => true,
                        "global::System.TimeOnly" => true,
                        // Add other supported types here if needed
                        _ => false
                    };
                }

                if (!isPrimitiveOrSupportedType)
                {
                    context.ReportDiagnostic(Diagnostic.Create(ErrorInvalidAttributeUsage,
                        typeDeclSyntax.Identifier.GetLocation(), attributeSymbol.Name, typeSymbol.Name,
                        $"The primitive type '{primitiveTypeSymbol.ToDisplayString()}' is not a supported primitive or system type"));
                    continue;
                }

                var predefinedInstances = new List<(string PropertyName, object Value)>();
                var userDefinedProperties = new HashSet<string>();

                GetPredefinedProperties(compilation, context, typeSymbol, userDefinedProperties, primitiveTypeSymbol,
                    predefinedInstances);

                if (ValidationChecks(context, attributeSymbol, typeDeclSyntax, typeSymbol, primitiveTypeSymbol))
                    continue;

                // Gather remaining info
                var typeName = typeSymbol.Name;
                var ns = typeSymbol.ContainingNamespace.IsGlobalNamespace
                    ? null
                    : typeSymbol.ContainingNamespace.ToDisplayString();
                var primitiveTypeName = primitiveTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var isValueType = typeSymbol.IsValueType;
                var typeKindKeyword = isValueType ? "readonly record struct" : "record class";
                var sealedModifier = !isValueType ? "sealed " : "";
                var systemTextConverterName = "SystemTextJsonConverter";
                var newtonsoftConverterName = "NewtonsoftJsonConverter";

                var hasNormalize = typeSymbol.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Any(m => m.Name == "Normalize" && m.Parameters.Length == 1 &&
                              m.Parameters[0].Type.ToDisplayString() == primitiveTypeName);

                var hasValidate = typeSymbol.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Any(m => m.Name == "Validate" && m.Parameters.Length == 1 &&
                              m.Parameters[0].Type.ToDisplayString()
                                  .Equals(primitiveTypeName, StringComparison.Ordinal) &&
                              m is { IsStatic: true, IsPartialDefinition: true });

                typesToProcess.Add(new WrapperTypeInfo(
                    TypeName: typeName,
                    Namespace: ns,
                    FullTypeName: fullTypeName,
                    PrimitiveTypeName: primitiveTypeName,
                    PrimitiveTypeSymbol: primitiveTypeSymbol,
                    IsValueType: isValueType,
                    TypeKindKeyword: typeKindKeyword,
                    SealedModifier: sealedModifier,
                    SystemTextConverterName: systemTextConverterName,
                    NewtonsoftConverterName: newtonsoftConverterName,
                    HasNormalizeImplementation: hasNormalize,
                    HasValidateImplementation: hasValidate,
                    PredefinedInstances: predefinedInstances,
                    UserDefinedProperties: userDefinedProperties
                ));
            }

            // At the end of the method, add the analyzer for partial methods for each valid type
            foreach (var typeInfo in typesToProcess)
            {
                var typeSyntax = types.FirstOrDefault(t =>
                    compilation.GetSemanticModel(t.SyntaxTree)
                        .GetDeclaredSymbol(t)?.ToDisplayString() == typeInfo.FullTypeName);

                if (typeSyntax != null)
                {
                    // This should be calling the instance method
                    ((ValueWrapperGenerator)null).AddPartialMethodAnalyzers(context, typeInfo, typeSyntax);
                }
            }
        }

        private static void GetPredefinedProperties(Compilation compilation, SourceProductionContext context,
            INamedTypeSymbol typeSymbol, HashSet<string> userDefinedProperties, ITypeSymbol primitiveTypeSymbol,
            List<(string PropertyName, object Value)> predefinedInstances
        )
        {
            // Collect static properties defined by the user to avoid generating duplicates
            foreach (var member in typeSymbol.GetMembers().OfType<IPropertySymbol>())
            {
                if (member.IsStatic)
                {
                    userDefinedProperties.Add(member.Name);
                }
            }

            // Find properties decorated with PredefinedValueAttribute
            foreach (var propertySymbol in typeSymbol.GetMembers().OfType<IPropertySymbol>())
            {
                if (!propertySymbol.IsStatic)
                {
                    continue;
                }

                var attr = propertySymbol.GetAttributes().FirstOrDefault(a =>
                    a.AttributeClass?.Name is "PredefinedValueAttribute" or "PredefinedValue");

                if (attr != null)
                {
                    // Check if the property is declared as partial in syntax
                    bool isPartialProperty = false;
                    foreach (var syntaxReference in propertySymbol.DeclaringSyntaxReferences)
                    {
                        if (syntaxReference.GetSyntax() is PropertyDeclarationSyntax propertySyntax &&
                            propertySyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                        {
                            isPartialProperty = true;
                            break;
                        }
                    }

                    if (!isPartialProperty)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            new DiagnosticDescriptor(
                                "VWG009",
                                "Invalid Predefined Property",
                                "Property '{0}' decorated with PredefinedValueAttribute must be declared with the 'partial' keyword",
                                "ValueWrapperGenerator",
                                DiagnosticSeverity.Error,
                                isEnabledByDefault: true),
                            propertySymbol.Locations.FirstOrDefault(),
                            propertySymbol.Name));
                        continue;
                    }

                    if (attr.ConstructorArguments.Length > 0 &&
                        attr.ConstructorArguments[0].Value is { } value)
                    {
                        var valueTypeSymbol = GetValueType(value, primitiveTypeSymbol, compilation);
                        bool isCompatible = false;

                        if (valueTypeSymbol != null)
                        {
                            isCompatible = SymbolEqualityComparer.Default.Equals(valueTypeSymbol, primitiveTypeSymbol);
                            if (!isCompatible && valueTypeSymbol.SpecialType != SpecialType.None &&
                                primitiveTypeSymbol.SpecialType != SpecialType.None)
                            {
                                isCompatible = CanImplicitlyConvert(valueTypeSymbol.SpecialType,
                                    primitiveTypeSymbol.SpecialType);
                            }
                            // Handle Guid specifically if primitive is Guid and value is string
                            else if (!isCompatible && primitiveTypeSymbol.ToDisplayString() == "global::System.Guid" &&
                                     value is string)
                            {
                                isCompatible = Guid.TryParse(value.ToString(), out _);
                                isCompatible = true;
                            }
                        }

                        if (isCompatible)
                        {
                            predefinedInstances.Add((propertySymbol.Name, value));
                        }
                        else
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                ErrorInvalidAttributeUsage,
                                propertySymbol.Locations.FirstOrDefault(),
                                "PredefinedValueAttribute",
                                typeSymbol.Name,
                                $"Value type '{(valueTypeSymbol?.ToDisplayString() ?? value?.GetType().Name ?? "unknown")}' for property '{propertySymbol.Name}' does not match or cannot be converted to the primitive type '{primitiveTypeSymbol.ToDisplayString()}'"));
                        }
                    }
                }
            }
        }

        private void AddPartialMethodAnalyzers(SourceProductionContext context, WrapperTypeInfo info,
            TypeDeclarationSyntax typeDecl
        )
        {
            // Check if user has implemented the Normalize method
            if (!info.HasNormalizeImplementation)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    InfoPartialMethodAvailable,
                    typeDecl.Identifier.GetLocation(),
                    "Normalize",
                    "normalization",
                    info.TypeName));
            }

            // Check if user has implemented the Validate method
            if (!info.HasValidateImplementation)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    InfoPartialMethodAvailable,
                    typeDecl.Identifier.GetLocation(),
                    "Validate",
                    "validation",
                    info.TypeName));
            }
        }

        private static bool ValidationChecks(SourceProductionContext context, INamedTypeSymbol attributeSymbol,
            TypeDeclarationSyntax typeDeclSyntax, INamedTypeSymbol typeSymbol, ITypeSymbol primitiveTypeSymbol
        )
        {
            // Existing validation checks
            if (!typeDeclSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            {
                context.ReportDiagnostic(Diagnostic.Create(ErrorNotPartial, typeDeclSyntax.Identifier.GetLocation(),
                    typeSymbol.Name, attributeSymbol.Name));
                return true;
            }

            if (!typeSymbol.IsRecord)
            {
                context.ReportDiagnostic(Diagnostic.Create(ErrorNotRecordClassOrStruct,
                    typeDeclSyntax.Identifier.GetLocation(), typeSymbol.Name, attributeSymbol.Name));
                return true;
            }

            if (typeSymbol.IsValueType && !typeDeclSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword)))
            {
                context.ReportDiagnostic(Diagnostic.Create(ErrorNotReadonlyStruct,
                    typeDeclSyntax.Identifier.GetLocation(), typeSymbol.Name, attributeSymbol.Name));
                return true;
            }

            if (primitiveTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                context.ReportDiagnostic(Diagnostic.Create(ErrorInvalidAttributeUsage,
                    typeDeclSyntax.Identifier.GetLocation(), attributeSymbol.Name, typeSymbol.Name,
                    $"The primitive type argument '{primitiveTypeSymbol.ToDisplayString()}' cannot be a nullable reference or value type"));
                return true;
            }

            return false;
        }

        // Add this helper method to check if source can implicitly convert to target
        private static bool CanImplicitlyConvert(SpecialType source, SpecialType target)
        {
            // Simple numeric conversion checks
            switch (source)
            {
                case SpecialType.System_Byte:
                case SpecialType.System_SByte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                    return target is SpecialType.System_Int32 or SpecialType.System_Int64 or
                        SpecialType.System_UInt32 or SpecialType.System_UInt64 or
                        SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal;

                case SpecialType.System_Int32:
                    return target is SpecialType.System_Int64 or SpecialType.System_Single or
                        SpecialType.System_Double or SpecialType.System_Decimal;

                case SpecialType.System_UInt32:
                    return target is SpecialType.System_Int64 or SpecialType.System_UInt64 or
                        SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal;

                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                    return target is SpecialType.System_Single or SpecialType.System_Double
                        or SpecialType.System_Decimal;

                case SpecialType.System_Char:
                    return target is SpecialType.System_Int32 or SpecialType.System_UInt32 or
                        SpecialType.System_Int64 or SpecialType.System_UInt64 or
                        SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal;

                case SpecialType.System_Single:
                    return target is SpecialType.System_Double;

                default:
                    return false;
            }
        }

        // Helper method to determine the type of the value
        private static ITypeSymbol? GetValueType(object value, ITypeSymbol expectedType, Compilation compilation)
        {
            if (value == null) return null;

            // For Guid specifically, handle string representations
            if (expectedType.ToDisplayString() == "System.Guid" && value is string guidStr)
            {
                // Just return the Guid type when a string is provided for a Guid parameter
                // The validation of the actual string happens elsewhere
                return expectedType;
            }

            return value switch
            {
                string => compilation.GetTypeByMetadataName("System.String"),
                int => compilation.GetTypeByMetadataName("System.Int32"),
                bool => compilation.GetTypeByMetadataName("System.Boolean"),
                char => compilation.GetTypeByMetadataName("System.Char"),
                long => compilation.GetTypeByMetadataName("System.Int64"),
                short => compilation.GetTypeByMetadataName("System.Int16"),
                byte => compilation.GetTypeByMetadataName("System.Byte"),
                float => compilation.GetTypeByMetadataName("System.Single"),
                double => compilation.GetTypeByMetadataName("System.Double"),
                decimal => compilation.GetTypeByMetadataName("System.Decimal"),
                Guid => compilation.GetTypeByMetadataName("System.Guid"),
                _ => null // Unsupported type, will trigger diagnostic
            };
        }

        private static void Phase2GenerateCode(SourceProductionContext context, List<WrapperTypeInfo> typesToProcess,
            Compilation compilation
        )
        {
            if (typesToProcess.Count <= 0) return;

            foreach (var typeInfo in typesToProcess)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    InfoGeneratedCode,
                    Location.None,
                    typeInfo.FullTypeName));

                // Generate the main implementation
                var generatedSource = GenerateImplementation(typeInfo, compilation);
                var safeFileName = typeInfo.FullTypeName.Replace("global::", "").Replace(".", "_").Replace(":", "_")
                    .Replace("<", "_").Replace(">", "_");
                context.AddSource($"{safeFileName}.g.cs", SourceText.From(generatedSource, Encoding.UTF8));
            }

            // Generate the central LiteDB registration file
            var registrationSource = CodeBuilder.GenerateLiteDbRegistrationImplementation(typesToProcess);
            context.AddSource("PrimifyLiteDbRegistration.g.cs",
                SourceText.From(registrationSource, Encoding.UTF8));
        }

        // Generates the implementation for a single type (record class or readonly record struct)
        private static string GenerateImplementation(WrapperTypeInfo info, Compilation compilation)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Diagnostics.CodeAnalysis;");
            sb.AppendLine("using System.Text.Json.Serialization;");
            sb.AppendLine("using NewtonsoftJson = Newtonsoft.Json;");

            // Check if LiteDB is referenced
            bool hasLiteDb = compilation.ReferencedAssemblyNames
                .Any(x => x.Name.Equals("LiteDB", StringComparison.OrdinalIgnoreCase));

            if (hasLiteDb)
            {
                sb.AppendLine("using LiteDB;");
            }

            var hasNamespace = !string.IsNullOrEmpty(info.Namespace);
            if (hasNamespace)
            {
                sb.AppendLine($"namespace {info.Namespace}");
                sb.AppendLine("{");
            }

            var indent = hasNamespace ? "    " : "";
            var nestedIndent = indent + "    ";
            var doubleNestedIndent = nestedIndent + "    ";

            CodeBuilder.AppendJsonConverterAttributesPointingToConverters(info, sb, indent);

            // Parse the TypeKindKeyword to insert 'partial' in the right place
            string typeDeclaration;
            if (info.TypeKindKeyword.Contains("record"))
            {
                // For "readonly record struct" or "record class", insert 'partial' before 'record'
                var parts = info.TypeKindKeyword.Split(["record"], StringSplitOptions.None);
                typeDeclaration = $"{parts[0]}partial record{parts[1]}";
            }
            else
            {
                // Fallback for non-record types
                typeDeclaration = $"partial {info.TypeKindKeyword}";
            }

            // Apply partial modifier to the type declaration
            sb.AppendLine($"{indent}public {info.SealedModifier}{typeDeclaration} {info.TypeName}");
            sb.AppendLine($"{indent}{{");

            // --- Property ---
            sb.AppendLine($"{nestedIndent}/// <summary>Gets the underlying primitive value.</summary>");
            sb.AppendLine($"{nestedIndent}public {info.PrimitiveTypeName} Value {{ get; }}");
            sb.AppendLine();

            CodeBuilder.AppendPredefinedInstancesImplementation(info, sb, nestedIndent);

            // --- Constructor ---
            sb.AppendLine(
                $"{nestedIndent}/// <summary>Creates a new instance with the specified primitive value.</summary>");
            sb.AppendLine($"{nestedIndent}private {info.TypeName}({info.PrimitiveTypeName} value)");
            sb.AppendLine($"{nestedIndent}{{");
            sb.AppendLine($"{doubleNestedIndent}Value = value;");
            sb.AppendLine($"{nestedIndent}}}");
            sb.AppendLine();

            CodeBuilder.AppendFactoryMethodImplementation(info, sb, nestedIndent, doubleNestedIndent);
            CodeBuilder.AppendImplicitExplicitConvertersImplementation(info, sb, nestedIndent);
            CodeBuilder.AppendNormalizeMethodImplementation(info, sb, nestedIndent);
            CodeBuilder.AppendValidateMethodImplementation(info, sb, nestedIndent);
            CodeBuilder.AppendToStringImplementation(info, sb, nestedIndent);
            CodeBuilder.AppendSystemTextJsonConverterImplementation(info, sb, nestedIndent, doubleNestedIndent);
            CodeBuilder.AppendNewtonsoftJsonConverterImplementation(info, sb, nestedIndent, doubleNestedIndent);
            CodeBuilder.AppendLiteDbRegistration(info, sb, nestedIndent, doubleNestedIndent);
            sb.AppendLine($"{indent}}}"); // Close partial type

            if (hasNamespace)
            {
                sb.AppendLine("}"); // Close namespace
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Internal record to store information about a type being processed.
    /// </summary>
    internal record WrapperTypeInfo(
        string TypeName,
        string? Namespace,
        string FullTypeName, // Fully qualified name (e.g., global::MyNamespace.MyType)
        string PrimitiveTypeName, // Fully qualified name (e.g., global::System.Guid)
        ITypeSymbol PrimitiveTypeSymbol,
        bool IsValueType, // True for struct/readonly record struct
        string TypeKindKeyword, // e.g., "readonly record struct", "record class"
        string SealedModifier, // "sealed " or ""
        string SystemTextConverterName,
        string NewtonsoftConverterName,
        bool HasNormalizeImplementation,
        bool HasValidateImplementation,
        List<(string PropertyName, object Value)>? PredefinedInstances = null,
        HashSet<string> UserDefinedProperties = null
    );
}
#endif

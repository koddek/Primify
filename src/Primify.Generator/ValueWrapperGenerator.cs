#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
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

            // Find our attribute type
            var attributeSymbol = compilation.GetTypeByMetadataName(AttributeFullName);
            if (attributeSymbol == null)
            {
                // Report diagnostic when attribute is not found
                context.ReportDiagnostic(Diagnostic.Create(ErrorAttributeNotFound,
                    Location.None, AttributeFullName));
                return;
            }

            var typesToProcess = new List<TypeInfo>();

            // --- Phase 1: Collect and Validate Types ---
            foreach (var typeDeclSyntax in types)
            {
                var semanticModel = compilation.GetSemanticModel(typeDeclSyntax.SyntaxTree);
                if (semanticModel.GetDeclaredSymbol(typeDeclSyntax) is not { } typeSymbol) continue;

                // Improve attribute lookup - check both constructed and original definition
                var attrData = typeSymbol.GetAttributes().FirstOrDefault(ad =>
                    ad.AttributeClass != null && (
                        ad.AttributeClass.Name.Contains("Primify") ||
                        (ad.AttributeClass.OriginalDefinition != null &&
                         SymbolEqualityComparer.Default.Equals(ad.AttributeClass.OriginalDefinition, attributeSymbol))
                    ));

                if (attrData == null) continue; // Not decorated with our attribute

                // Validate: Must be partial
                if (!typeDeclSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(ErrorNotPartial, typeDeclSyntax.Identifier.GetLocation(),
                        typeSymbol.Name, attributeSymbol.Name));
                    continue;
                }

                // Validate: Must be a record class or record struct
                if (!typeSymbol.IsRecord)
                {
                    context.ReportDiagnostic(Diagnostic.Create(ErrorNotRecordClassOrStruct,
                        typeDeclSyntax.Identifier.GetLocation(), typeSymbol.Name, attributeSymbol.Name));
                    continue;
                }

                // Validate: Struct must be readonly
                if (typeSymbol.IsValueType && !typeDeclSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(ErrorNotReadonlyStruct,
                        typeDeclSyntax.Identifier.GetLocation(), typeSymbol.Name, attributeSymbol.Name));
                    continue;
                }

                // Validate: Attribute has one type argument
                if (attrData.AttributeClass?.TypeArguments.Length != 1)
                {
                    context.ReportDiagnostic(Diagnostic.Create(ErrorInvalidAttributeUsage,
                        typeDeclSyntax.Identifier.GetLocation(), attributeSymbol.Name, typeSymbol.Name,
                        "Requires exactly one type argument (the primitive type)"));
                    continue;
                }

                var primitiveTypeSymbol = attrData.AttributeClass.TypeArguments[0];

                // Validate: Primitive type cannot be nullable directly in attribute
                if (primitiveTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
                {
                    context.ReportDiagnostic(Diagnostic.Create(ErrorInvalidAttributeUsage,
                        typeDeclSyntax.Identifier.GetLocation(), attributeSymbol.Name, typeSymbol.Name,
                        $"The primitive type argument '{primitiveTypeSymbol.ToDisplayString()}' cannot be a nullable reference or value type"));
                    continue;
                }

                // --- Gather Info ---
                var typeName = typeSymbol.Name;
                var ns = typeSymbol.ContainingNamespace.IsGlobalNamespace
                    ? null
                    : typeSymbol.ContainingNamespace.ToDisplayString();
                var primitiveTypeName =
                    primitiveTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var isValueType = typeSymbol.IsValueType; // struct or record struct

                // Determine C# keyword(s)
                var typeKindKeyword = isValueType ? "readonly record struct" : "record class";

                // Default to sealed for classes
                var sealedModifier = !isValueType ? "sealed " : "";

                // Determine converter names (nested)
                var systemTextConverterName = "SystemTextJsonConverter";
                var newtonsoftConverterName = "NewtonsoftJsonConverter";

                // --- Detect existing Normalize and Validate implementations ---
                var hasNormalize = typeSymbol
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .Any(m => m.Name == "Normalize" && m.Parameters.Length == 1 &&
                              m.Parameters[0].Type.ToDisplayString() == primitiveTypeName);

                var hasValidate = typeSymbol
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .Any(m => m.Name == "Validate" && m.Parameters.Length == 1 &&
                              m.Parameters[0].Type.ToDisplayString() == primitiveTypeName);

                typesToProcess.Add(new TypeInfo(
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
                    HasValidateImplementation: hasValidate
                ));
            }

            // --- Phase 2: Generate Code ---
            if (typesToProcess.Count > 0)
            {
                foreach (var typeInfo in typesToProcess)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        InfoGeneratedCode,
                        Location.None,
                        typeInfo.FullTypeName));

                    // Generate the main implementation
                    var generatedSource = GenerateImplementation(typeInfo);
                    var safeFileName = typeInfo.FullTypeName.Replace("global::", "").Replace(".", "_").Replace(":", "_")
                        .Replace("<", "_").Replace(">", "_");
                    context.AddSource($"{safeFileName}.g.cs", SourceText.From(generatedSource, Encoding.UTF8));
                }

                // Generate the central LiteDB registration file
                var registrationSource = GenerateLiteDbRegistration(typesToProcess);
                context.AddSource("PrimifyLiteDbRegistration.g.cs",
                    SourceText.From(registrationSource, Encoding.UTF8));
            }
        }

        // Generates the implementation for a single type (record class or readonly record struct)
        private static string GenerateImplementation(TypeInfo info)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Diagnostics.CodeAnalysis; // For NotNullWhen attribute");
            sb.AppendLine("using System.Text.Json.Serialization;");
            sb.AppendLine("using NewtonsoftJson = Newtonsoft.Json;");
            sb.AppendLine();

            var hasNamespace = !string.IsNullOrEmpty(info.Namespace);
            if (hasNamespace)
            {
                sb.AppendLine($"namespace {info.Namespace}");
                sb.AppendLine("{");
            }

            var indent = hasNamespace ? "    " : "";
            var nestedIndent = indent + "    ";
            var doubleNestedIndent = nestedIndent + "    ";

            // Apply JsonConverter attributes pointing to nested converters
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Represents a value wrapper for <see cref=\"{info.PrimitiveTypeName}\"/>.");
            sb.AppendLine($"{indent}/// Generated by ValueWrapperGenerator.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}[JsonConverter(typeof({info.TypeName}.{info.SystemTextConverterName}))]");
            sb.AppendLine(
                $"{indent}[NewtonsoftJson.JsonConverter(typeof({info.TypeName}.{info.NewtonsoftConverterName}))]");

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

            // Conditional Normalize implementation
            sb.AppendLine(
                $"{nestedIndent}/// <summary>Method for normalizing the primitive value during construction.</summary>");
            // If user has a partial implementation, just declare the stub
            sb.AppendLine(
                info.HasNormalizeImplementation
                    ? $"{nestedIndent}private static partial {info.PrimitiveTypeName} Normalize({info.PrimitiveTypeName} value);"
                    // If no user implementation, provide the complete method
                    : $"{nestedIndent}static {info.PrimitiveTypeName} Normalize({info.PrimitiveTypeName} value) => value;");

            sb.AppendLine();

            // Conditional Validate implementation
            sb.AppendLine(
                $"{nestedIndent}/// <summary>Method for validating the primitive value during construction.</summary>");
            // If user has a partial implementation, just declare the stub
            sb.AppendLine(info.HasValidateImplementation
                ? $"{nestedIndent}static partial void Validate({info.PrimitiveTypeName} value);"
                // If no user implementation, provide a no-op implementation
                : $"{nestedIndent}static void Validate({info.PrimitiveTypeName} value) {{ /* No validation by default */ }}");

            sb.AppendLine();

            // --- Constructor ---
            sb.AppendLine(
                $"{nestedIndent}/// <summary>Creates a new instance with the specified primitive value.</summary>");
            sb.AppendLine($"{nestedIndent}private {info.TypeName}({info.PrimitiveTypeName} value)");
            sb.AppendLine($"{nestedIndent}{{");
            sb.AppendLine($"{doubleNestedIndent}Value = value;");
            sb.AppendLine($"{nestedIndent}}}");
            sb.AppendLine();

            // --- Factory Method ---
            sb.AppendLine($"{nestedIndent}/// <summary>Creates a new instance from a primitive value.</summary>");
            sb.AppendLine($"{nestedIndent}public static {info.TypeName} From({info.PrimitiveTypeName} value)");
            sb.AppendLine($"{nestedIndent}{{");
            sb.AppendLine($"{doubleNestedIndent}var normalized = Normalize(value);");
            sb.AppendLine($"{doubleNestedIndent}Validate(normalized);");
            sb.AppendLine($"{doubleNestedIndent}return new {info.TypeName}(normalized);");
            sb.AppendLine($"{nestedIndent}}}");
            sb.AppendLine();

            // --- Implicit Conversions ---
            sb.AppendLine(
                $"{nestedIndent}/// <summary>Implicitly converts the wrapper to its primitive value.</summary>");
            sb.AppendLine(
                $"{nestedIndent}public static implicit operator {info.PrimitiveTypeName}({info.TypeName} self) => self.Value;");
            sb.AppendLine();
            sb.AppendLine(
                $"{nestedIndent}/// <summary>Implicitly converts a primitive value to the wrapper type.</summary>");
            sb.AppendLine(
                $"{nestedIndent}public static implicit operator {info.TypeName}({info.PrimitiveTypeName} value) => From(value);");
            sb.AppendLine();

            // --- System.Text.Json Converter ---
            sb.AppendLine($"{nestedIndent}/// <summary>Internal System.Text.Json converter.</summary>");
            sb.AppendLine(
                $"{nestedIndent}internal class {info.SystemTextConverterName} : JsonConverter<{info.TypeName}>");
            sb.AppendLine($"{nestedIndent}{{");
            // Return type is non-nullable for structs, nullable for classes
            var readReturnType = info.IsValueType ? info.TypeName : $"{info.TypeName}?";
            sb.AppendLine(
                $"{doubleNestedIndent}public override {readReturnType} Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)");
            sb.AppendLine($"{doubleNestedIndent}{{");
            sb.AppendLine($"{doubleNestedIndent}    if (reader.TokenType == System.Text.Json.JsonTokenType.Null)");
            sb.AppendLine($"{doubleNestedIndent}    {{");
            // Return null for classes, default (which is default struct) for structs
            sb.AppendLine(
                $"{doubleNestedIndent}        return {(info.IsValueType ? $"default({info.TypeName})" : "null")};");
            sb.AppendLine($"{doubleNestedIndent}    }}");
            sb.AppendLine(
                $"{doubleNestedIndent}    var primitiveValue = System.Text.Json.JsonSerializer.Deserialize<{info.PrimitiveTypeName}>(ref reader, options);");
            sb.AppendLine($"{doubleNestedIndent}    // Use factory method instead of constructor for validation");
            sb.AppendLine($"{doubleNestedIndent}    return {info.TypeName}.From(primitiveValue!);");
            sb.AppendLine($"{doubleNestedIndent}}}");
            sb.AppendLine();
            sb.AppendLine(
                $"{doubleNestedIndent}public override void Write(System.Text.Json.Utf8JsonWriter writer, {info.TypeName} value, System.Text.Json.JsonSerializerOptions options)");
            sb.AppendLine($"{doubleNestedIndent}{{");
            // Handle null classes passed to Write (structs cannot be null here)
            if (!info.IsValueType)
            {
                sb.AppendLine($"{doubleNestedIndent}    if (value is null)");
                sb.AppendLine($"{doubleNestedIndent}    {{");
                sb.AppendLine($"{doubleNestedIndent}        writer.WriteNullValue();");
                sb.AppendLine($"{doubleNestedIndent}        return;");
                sb.AppendLine($"{doubleNestedIndent}    }}");
            }

            sb.AppendLine($"{doubleNestedIndent}    // Use implicit conversion to get primitive");
            sb.AppendLine(
                $"{doubleNestedIndent}    System.Text.Json.JsonSerializer.Serialize(writer, ({info.PrimitiveTypeName})value, options);");
            sb.AppendLine($"{doubleNestedIndent}}}");
            sb.AppendLine($"{nestedIndent}}}"); // Close SystemTextJsonConverter
            sb.AppendLine();

            // --- Newtonsoft.Json Converter ---
            sb.AppendLine($"{nestedIndent}/// <summary>Internal Newtonsoft.Json converter.</summary>");
            if (info.IsValueType)
            {
                // Struct implementation - No nullable parameter types
                sb.AppendLine(
                    $"{nestedIndent}internal class {info.NewtonsoftConverterName} : NewtonsoftJson.JsonConverter<{info.TypeName}>");
                sb.AppendLine($"{nestedIndent}{{");
                sb.AppendLine(
                    $"{doubleNestedIndent}public override void WriteJson(NewtonsoftJson.JsonWriter writer, {info.TypeName} value, NewtonsoftJson.JsonSerializer serializer)");
                sb.AppendLine($"{doubleNestedIndent}{{");
                sb.AppendLine($"{doubleNestedIndent}    // Structs cannot be null, so no null check needed");
                sb.AppendLine($"{doubleNestedIndent}    // Use implicit conversion to get primitive");
                sb.AppendLine(
                    $"{doubleNestedIndent}    serializer.Serialize(writer, ({info.PrimitiveTypeName})value);");
                sb.AppendLine($"{doubleNestedIndent}}}");
                sb.AppendLine();
                sb.AppendLine(
                    $"{doubleNestedIndent}public override {info.TypeName} ReadJson(NewtonsoftJson.JsonReader reader, Type objectType, {info.TypeName} existingValue, bool hasExistingValue, NewtonsoftJson.JsonSerializer serializer)");
                sb.AppendLine($"{doubleNestedIndent}{{");
                sb.AppendLine($"{doubleNestedIndent}    if (reader.TokenType == NewtonsoftJson.JsonToken.Null)");
                sb.AppendLine($"{doubleNestedIndent}    {{");
                sb.AppendLine($"{doubleNestedIndent}        // Return default value for struct");
                sb.AppendLine($"{doubleNestedIndent}        return default;");
                sb.AppendLine($"{doubleNestedIndent}    }}");
                sb.AppendLine(
                    $"{doubleNestedIndent}    var primitiveValue = serializer.Deserialize<{info.PrimitiveTypeName}>(reader);");
                sb.AppendLine($"{doubleNestedIndent}    // Use factory method for validation");
                sb.AppendLine($"{doubleNestedIndent}    return {info.TypeName}.From(primitiveValue!);");
                sb.AppendLine($"{doubleNestedIndent}}}");
            }
            else
            {
                // Class implementation - With nullable parameter types
                sb.AppendLine(
                    $"{nestedIndent}internal class {info.NewtonsoftConverterName} : NewtonsoftJson.JsonConverter<{info.TypeName}>");
                sb.AppendLine($"{nestedIndent}{{");
                sb.AppendLine(
                    $"{doubleNestedIndent}public override void WriteJson(NewtonsoftJson.JsonWriter writer, {info.TypeName}? value, NewtonsoftJson.JsonSerializer serializer)");
                sb.AppendLine($"{doubleNestedIndent}{{");
                sb.AppendLine($"{doubleNestedIndent}    if (value is null)");
                sb.AppendLine($"{doubleNestedIndent}    {{");
                sb.AppendLine($"{doubleNestedIndent}        writer.WriteNull();");
                sb.AppendLine($"{doubleNestedIndent}        return;");
                sb.AppendLine($"{doubleNestedIndent}    }}");
                sb.AppendLine($"{doubleNestedIndent}    // Use implicit conversion to get primitive");
                sb.AppendLine(
                    $"{doubleNestedIndent}    serializer.Serialize(writer, ({info.PrimitiveTypeName})value);");
                sb.AppendLine($"{doubleNestedIndent}}}");
                sb.AppendLine();
                sb.AppendLine(
                    $"{doubleNestedIndent}public override {info.TypeName}? ReadJson(NewtonsoftJson.JsonReader reader, Type objectType, {info.TypeName}? existingValue, bool hasExistingValue, NewtonsoftJson.JsonSerializer serializer)");
                sb.AppendLine($"{doubleNestedIndent}{{");
                sb.AppendLine($"{doubleNestedIndent}    if (reader.TokenType == NewtonsoftJson.JsonToken.Null)");
                sb.AppendLine($"{doubleNestedIndent}    {{");
                sb.AppendLine($"{doubleNestedIndent}        return null;");
                sb.AppendLine($"{doubleNestedIndent}    }}");
                sb.AppendLine(
                    $"{doubleNestedIndent}    var primitiveValue = serializer.Deserialize<{info.PrimitiveTypeName}>(reader);");
                sb.AppendLine($"{doubleNestedIndent}    // Use factory method for validation");
                sb.AppendLine($"{doubleNestedIndent}    return {info.TypeName}.From(primitiveValue!);");
                sb.AppendLine($"{doubleNestedIndent}}}");
            }

            sb.AppendLine($"{nestedIndent}}}"); // Close NewtonsoftJsonConverter

            sb.AppendLine($"{indent}}}"); // Close partial type

            if (hasNamespace)
            {
                sb.AppendLine("}"); // Close namespace
            }

            return sb.ToString();
        }

        // Generates the central registration class ONLY for BSON mapping
        private string GenerateLiteDbRegistration(List<TypeInfo> types)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using System;");
            sb.AppendLine("using LiteDB;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine();
            // Add using directives for all namespaces containing the types
            foreach (var ns in types.Select(t => t.Namespace).Where(n => !string.IsNullOrEmpty(n)).Distinct())
            {
                sb.AppendLine($"using {ns};");
            }

            sb.AppendLine();
            sb.AppendLine("namespace Primify.Generated");
            sb.AppendLine("{");
            sb.AppendLine(
                "    /// <summary>Provides BSON registration for types generated by ValueWrapperGenerator.</summary>");
            sb.AppendLine("    internal static class PrimifyLiteDbRegistration");
            sb.AppendLine("    {");
            sb.AppendLine("        private static bool _initialized = false;");
            sb.AppendLine();
            sb.AppendLine("        /// <summary>Initializes BSON mappers using BsonMapper.Global.</summary>");
            sb.AppendLine("        [ModuleInitializer]");
            sb.AppendLine("        internal static void InitializeGlobalLiteDbBsonMappers()");
            sb.AppendLine("        {");
            sb.AppendLine("            RegisterLiteDbBsonMappers(BsonMapper.Global);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        /// <summary>Registers BSON mappers with a specific BsonMapper instance.</summary>");
            sb.AppendLine("        public static void RegisterLiteDbBsonMappers(BsonMapper mapper)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (mapper == BsonMapper.Global && _initialized) return;");
            sb.AppendLine("            if (mapper == null) throw new ArgumentNullException(nameof(mapper));");
            sb.AppendLine();
            foreach (var info in types)
            {
                string deserializeExpression;
                string serializeExpression;

                // Special handling for different primitive types
                if (info.PrimitiveTypeSymbol.ToDisplayString() == "System.Guid")
                {
                    // Special handling for Guid
                    serializeExpression = $"new BsonValue(({info.PrimitiveTypeName})value)";
                    deserializeExpression = $"{info.FullTypeName}.From(bson.AsGuid)";
                }
                else if (info.PrimitiveTypeSymbol.SpecialType == SpecialType.System_DateTime)
                {
                    serializeExpression = $"new BsonValue(({info.PrimitiveTypeName})value)";
                    deserializeExpression = $"{info.FullTypeName}.From(bson.AsDateTime)";
                }
                else
                {
                    // Standard handling for other types
                    var bsonAccessor = GetBsonAccessor(info.PrimitiveTypeSymbol);
                    serializeExpression = $"new BsonValue(({info.PrimitiveTypeName})value)";
                    deserializeExpression = $"{info.FullTypeName}.From(bson.{bsonAccessor})";

                    if (bsonAccessor == "RawValue")
                    {
                        sb.AppendLine(
                            $"            // Warning: BSON mapping for {info.PrimitiveTypeName} used by {info.FullTypeName} using RawValue.");
                    }
                }

                sb.AppendLine($"            mapper.RegisterType<{info.FullTypeName}>(");
                sb.AppendLine($"                serialize: (value) => {serializeExpression},");
                sb.AppendLine($"                deserialize: (bson) => {deserializeExpression});");
            }

            sb.AppendLine();
            sb.AppendLine("            if (mapper == BsonMapper.Global) _initialized = true;");
            sb.AppendLine("        }");
            sb.AppendLine("    }"); // Close class
            sb.AppendLine("}"); // Close namespace

            return sb.ToString();
        }

        // Helper to get the LiteDB BsonValue accessor
        private string GetBsonAccessor(ITypeSymbol primitiveTypeSymbol)
        {
            switch (primitiveTypeSymbol.SpecialType)
            {
                case SpecialType.System_Boolean: return "AsBoolean";
                case SpecialType.System_Char: return "AsString";
                case SpecialType.System_SByte: return "AsInt32";
                case SpecialType.System_Byte: return "AsInt32";
                case SpecialType.System_Int16: return "AsInt32";
                case SpecialType.System_UInt16: return "AsInt32";
                case SpecialType.System_Int32: return "AsInt32";
                case SpecialType.System_UInt32: return "AsInt32";
                case SpecialType.System_Int64: return "AsInt64";
                case SpecialType.System_UInt64: return "AsDecimal";
                case SpecialType.System_Single: return "AsDecimal";
                case SpecialType.System_Double: return "AsDecimal";
                case SpecialType.System_Decimal: return "AsDecimal";
                case SpecialType.System_String: return "AsString";
                case SpecialType.System_DateTime: return "AsDateTime";
                default:
                    if (primitiveTypeSymbol.ToDisplayString() == "System.Guid")
                        return "AsGuid";
                    if (primitiveTypeSymbol.ToDisplayString() == "System.DateTimeOffset")
                        return "AsDateTime";
                    if (primitiveTypeSymbol.ToDisplayString() == "System.TimeSpan")
                        return "AsInt64";
                    if (primitiveTypeSymbol.TypeKind == TypeKind.Enum)
                        return "AsInt32";
                    return "RawValue"; // Fallback, might not work for all types
            }
        }

        /// <summary>
        /// Internal record to store information about a type being processed.
        /// </summary>
        private record TypeInfo(
            string TypeName,
            string? Namespace,
            string FullTypeName,
            string PrimitiveTypeName,
            ITypeSymbol PrimitiveTypeSymbol,
            bool IsValueType,
            string TypeKindKeyword,
            string SealedModifier,
            string SystemTextConverterName,
            string NewtonsoftConverterName,
            bool HasNormalizeImplementation,
            bool HasValidateImplementation
        );
    }
}
#endif

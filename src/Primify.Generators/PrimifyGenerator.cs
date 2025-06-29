using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Primify.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class PrimifyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. Post-initialize the attribute source code

        // 2. Define the type provider AFTER the attribute has been added to the compilation
        var typeProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (s, _) => IsCandidateTypeDeclaration(s),
                transform: (ctx, _) => GetTypeDeclarationForSourceGen(ctx)
            )
            .Where(t => t.ReportAttributeFound);

        // 3. Generate the source code.
        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(typeProvider.Collect()),
            ((ctx, t) => GenerateCode(ctx, t.Left, t.Right)));
    }

    private static bool IsCandidateTypeDeclaration(SyntaxNode node)
    {
        // First check if it's a type declaration
        if (node is not TypeDeclarationSyntax typeDeclaration)
        {
            return false;
        }

        // Quick syntax check for supported types
        if (typeDeclaration is not (ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax))
        {
            return false;
        }

        // Quick check for partial keyword
        if (!typeDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
        {
            return false;
        }

        // Quick check for any attributes
        if (typeDeclaration.AttributeLists.Count == 0)
        {
            return false;
        }

        // Quick check for potential Primify attribute
        return typeDeclaration.AttributeLists
            .Any(attrList => attrList.Attributes
                .Select(attr => attr.Name.ToString())
                .Any(name => name == Names.PrimifyAttName ||
                             name == Names.PrimifyAttNameWithPostfix ||
                             name.StartsWith(Names.PrimifyAttName + "<")));
    }

    private static TypeDeclarationWithDiagnostics GetTypeDeclarationForSourceGen(
        GeneratorSyntaxContext context)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;
        var diagnostics = ImmutableArray<Diagnostic>.Empty;
        var hasWrapAttribute = false;

        // Check if it's a type we care about
        if (typeDeclaration is not (ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax))
        {
            diagnostics = diagnostics.Add(Diagnostics.CreateUnsupportedTypeKind(
                typeDeclaration.Identifier.GetLocation(),
                typeDeclaration.Identifier.Text));
            return new TypeDeclarationWithDiagnostics(typeDeclaration, hasWrapAttribute, diagnostics);
        }

        // Check if it has the partial keyword
        if (!typeDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
        {
            diagnostics = diagnostics.Add(Diagnostics.CreateMissingPartialModifier(
                typeDeclaration.Identifier.GetLocation(),
                typeDeclaration.Identifier.Text));
            return new TypeDeclarationWithDiagnostics(typeDeclaration, hasWrapAttribute, diagnostics);
        }

        // Check if it has any attributes
        if (typeDeclaration.AttributeLists.Count == 0)
        {
            diagnostics = diagnostics.Add(Diagnostics.CreateNoAttributesFound(
                typeDeclaration.Identifier.GetLocation(),
                typeDeclaration.Identifier.Text));
            return new TypeDeclarationWithDiagnostics(typeDeclaration, hasWrapAttribute, diagnostics);
        }

        // Look for Primify attribute
        var hasWrapAttributeDirect = false;
        foreach (var attribute in typeDeclaration.AttributeLists.SelectMany(al => al.Attributes))
        {
            if (ModelExtensions.GetSymbolInfo(context.SemanticModel, attribute).Symbol is not IMethodSymbol
                attributeSymbol)
                continue;

            var attributeName = attributeSymbol.ContainingType.ToDisplayString();
            if (attributeName.StartsWith(Names.PrimifyAttFullName))
            {
                hasWrapAttribute = true;
                hasWrapAttributeDirect = true;
                break;
            }
        }

        if (!hasWrapAttributeDirect)
        {
            diagnostics = diagnostics.Add(Diagnostics.CreateMissingWrapAttribute(
                typeDeclaration.Identifier.GetLocation(),
                typeDeclaration.Identifier.Text));
        }

        return new TypeDeclarationWithDiagnostics(typeDeclaration, hasWrapAttribute, diagnostics);
    }

    private static void GenerateCode(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<TypeDeclarationWithDiagnostics> typeDeclarations
    )
    {
        // Report all diagnostics first
        foreach (var diagnostic in typeDeclarations.SelectMany(type => type.Diagnostics))
        {
            context.ReportDiagnostic(diagnostic);
        }

        // Go through all filtered class declarations.
        foreach (var typeWithDiag in typeDeclarations)
        {
            var typeDeclarationSyntax = typeWithDiag.TypeDeclaration;
            var semanticModel = compilation.GetSemanticModel(typeDeclarationSyntax.SyntaxTree);
            if (ModelExtensions.GetDeclaredSymbol(semanticModel, typeDeclarationSyntax) is not INamedTypeSymbol
                typeSymbol)
                continue;

            var hasNormalizeImplementation = typeSymbol.GetMembers("Normalize")
                .OfType<IMethodSymbol>()
                .Any(m => !m.IsAbstract && m is
                    { IsVirtual: false, IsOverride: false, PartialImplementationPart: null });

            var hasValidateImplementation = typeSymbol.GetMembers("Validate")
                .OfType<IMethodSymbol>()
                .Any(m => !m.IsAbstract && m is
                    { IsVirtual: false, IsOverride: false, PartialImplementationPart: null });

            var namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();

            var typeKeyword = typeDeclarationSyntax switch
            {
                ClassDeclarationSyntax => "class",
                StructDeclarationSyntax => "struct",
                RecordDeclarationSyntax record => record.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword)
                    ? "record class"
                    : "record struct",
                _ => "class"
            };

            var wrapperName = typeDeclarationSyntax.Identifier.Text;

            var attribute = typeSymbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name.StartsWith("PrimifyAttribute") == true);

            var wrapperArgument = attribute?.AttributeClass?.TypeArguments.FirstOrDefault()?.ToDisplayString() ??
                                  "object";

            // *** LOGIC FLAGS ***
            var isRecord = typeDeclarationSyntax is RecordDeclarationSyntax;
            var isValueType = typeSymbol.IsValueType;

            // ** ALWAYS ENFORCE sealed/readonly MODIFIERS based on the final type **
            var finalModifier = isValueType ? "readonly" : "sealed";

            var equalityMembers = GenerateEqualityMembers(wrapperName, wrapperArgument, isRecord, isValueType);
            var toStringOverride = GenerateToStringOverride();
            var liteDbInitializer = GenerateLiteDbInitializer(wrapperName, wrapperArgument);
            var implicitCasting = GenerateImplicitCasting(wrapperName, wrapperArgument);
            var explicitCasting = GenerateExplicitCasting(wrapperName, wrapperArgument);

            // Build up the source code
            var code = $$"""
                         // <auto-generated/>
                         #nullable enable

                         using System;
                         using System.Runtime.CompilerServices;
                         using LiteDB;
                         using Primify.Converters;

                         namespace {{namespaceName}};

                         {{liteDbInitializer}}

                         [System.Text.Json.Serialization.JsonConverter(typeof(SystemTextJsonConverter<{{wrapperName}}, {{wrapperArgument}}>))]
                         [Newtonsoft.Json.JsonConverter(typeof(NewtonsoftJsonConverter<{{wrapperName}}, {{wrapperArgument}}>))]
                         [LiteDbSerializable]
                         {{finalModifier}} partial {{typeKeyword}} {{wrapperName}} : IPrimify<{{wrapperName}}, {{wrapperArgument}}>, IEquatable<{{wrapperName}}>
                         {
                             public {{wrapperArgument}} Value { get; }
                             
                             private {{wrapperName}}({{wrapperArgument}} value) => Value = value;

                             public static {{wrapperName}} From({{wrapperArgument}} value)
                             {
                                 var processedValue = value;

                                 // Enable Normalize only when the user has an implementation
                                 {{(hasNormalizeImplementation ? "" : "// ")}}processedValue = Normalize(processedValue);
                                 
                                 // Enable Validate only when the user has an implementation
                                 {{(hasValidateImplementation ? "" : "// ")}}Validate(processedValue);
                                 
                                 return new {{wrapperName}}(processedValue);
                             }
                             
                         {{implicitCasting}}

                         {{explicitCasting}}
                             
                         {{toStringOverride}}

                         {{equalityMembers}}
                         }
                         """;

            context.AddSource($"{namespaceName}.{wrapperName}.g.cs", SourceText.From(code, Encoding.UTF8));
        }
    }

    private static string GenerateEqualityMembers(string name, string argument, bool isRecord, bool isValueType)
    {
        if (isRecord)
        {
            // For RECORD CLASS and RECORD STRUCT:
            // We MUST override the compiler-synthesized members to enforce Value-only equality.
            // We DO NOT generate operators (==, !=), as the compiler's versions will correctly call our overridden Equals method.
            var equatableEquals = isValueType
                ? $"public bool Equals({name} other)" // record struct has a non-nullable Equals(T)
                : $"public bool Equals({name}? other)"; // record class has an overridable Equals(T?)

            return $$"""
                         //~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~
                         // Equality members for a RECORD, overriding default behavior.
                         //~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~
                         public override int GetHashCode() => System.Collections.Generic.EqualityComparer<{{argument}}>.Default.GetHashCode(Value);

                         {{equatableEquals}}
                         {
                             {{(isValueType ? "" : "if (other is null) return false;\n")}}
                             return System.Collections.Generic.EqualityComparer<{{argument}}>.Default.Equals(Value, other.Value);
                         }
                     """;
        }

        if (isValueType)
        {
            // For a plain STRUCT (which will be readonly)
            return $$"""
                         //~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~
                         // Equality members for a value type (struct).
                         //~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~
                         public override bool Equals(object? obj) => obj is {{name}} other && Equals(other);

                         public bool Equals({{name}} other)
                         {
                             return System.Collections.Generic.EqualityComparer<{{argument}}>.Default.Equals(Value, other.Value);
                         }

                         public override int GetHashCode() => System.Collections.Generic.EqualityComparer<{{argument}}>.Default.GetHashCode(Value);

                         public static bool operator ==({{name}} left, {{name}} right) => left.Equals(right);

                         public static bool operator !=({{name}} left, {{name}} right) => !(left == right);
                     """;
        }

        // For a plain, non-record CLASS (which will be sealed)
        return $$"""
                     //~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~
                     // Equality members for a sealed reference type (class).
                     //~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~
                     public override bool Equals(object? obj) => obj is {{name}} other && Equals(other);

                     public bool Equals({{name}}? other)
                     {
                         if (ReferenceEquals(null, other)) return false;
                         if (ReferenceEquals(this, other)) return true;
                        
                         return System.Collections.Generic.EqualityComparer<{{argument}}>.Default.Equals(Value, other.Value);
                     }

                     public override int GetHashCode() => System.Collections.Generic.EqualityComparer<{{argument}}>.Default.GetHashCode(Value);

                     public static bool operator ==({{name}}? left, {{name}}? right) =>
                         ReferenceEquals(left, right) || (left is not null && left.Equals(right));

                     public static bool operator !=({{name}}? left, {{name}}? right) => !(left == right);
                 """;
    }

    private static string GenerateToStringOverride() =>
        """
            /// <summary>
            /// Returns the string representation of the wrapped value.
            /// </summary>
            public override string ToString() => Value.ToString();
        """;

    private static string GenerateExplicitCasting(string name, string argument) =>
        $"""
             public static explicit operator {name}({argument} value) => From(value);
             public static explicit operator {argument}({name} value) => value.Value;
         """;

    private static string GenerateImplicitCasting(string name, string argument)
    {
        string toBson;
        // We will generate the "from BsonValue" part differently based on complexity.
        string fromBsonImplementation;

        switch (argument)
        {
            case "System.DateOnly":
                toBson = "new LiteDB.BsonValue(value.Value.ToDateTime(System.TimeOnly.MinValue))";
                fromBsonImplementation = $"=> {name}.From(System.DateOnly.FromDateTime(value.AsDateTime));";
                break;

            case "System.TimeOnly":
                toBson = "new LiteDB.BsonValue(value.Value.Ticks)";
                fromBsonImplementation = $"=> {name}.From(new System.TimeOnly(value.AsInt64));";
                break;

            case "System.DateTimeOffset":
                // Create a BsonDocument for serialization
                toBson = """
                             new LiteDB.BsonDocument
                             {
                                 ["DateTime"] = value.Value.UtcDateTime,
                                 ["Offset"] = value.Value.Offset.Ticks
                             }
                         """;

                // Generate a full method body for deserialization
                fromBsonImplementation = $$"""
                                           {
                                               var doc = value.AsDocument;
                                               var utcDateTime = doc["DateTime"].AsDateTime;
                                               var offset = new System.TimeSpan(doc["Offset"].AsInt64);

                                               // Create a UTC DateTimeOffset first, then convert to the original offset
                                               var utcTime = new System.DateTimeOffset(utcDateTime);
                                               var originalTime = utcTime.ToOffset(offset);

                                               return {{name}}.From(originalTime);
                                           }
                                           """;
                break;

            default:
                toBson = "new LiteDB.BsonValue(value.Value)";
                fromBsonImplementation =
                    $"=> {name}.From(({argument})System.Convert.ChangeType(value.RawValue, typeof({argument})));";
                break;
        }

        return $"""
                    // Casting for BSON
                    public static implicit operator LiteDB.BsonValue({name} value) =>
                        {toBson};

                    public static implicit operator {name}(LiteDB.BsonValue value)
                        {fromBsonImplementation}
                """;
    }

    private static string GenerateLiteDbInitializer(string name, string argument)
    {
        string serializeCode;
        string deserializeCode;

        switch (argument)
        {
            case "System.DateOnly":
                serializeCode = "wrapper => new LiteDB.BsonValue(wrapper.Value.ToDateTime(System.TimeOnly.MinValue))";
                deserializeCode = $"bson => {name}.From(System.DateOnly.FromDateTime(bson.AsDateTime))";
                break;

            case "System.TimeOnly":
                serializeCode = "wrapper => new LiteDB.BsonValue(wrapper.Value.Ticks)";
                deserializeCode = $"bson => {name}.From(new System.TimeOnly(bson.AsInt64))";
                break;

            case "System.DateTimeOffset":
                serializeCode = """
                                wrapper =>
                                            new LiteDB.BsonDocument
                                            {
                                                ["DateTime"] = wrapper.Value.UtcDateTime,
                                                ["Offset"] = wrapper.Value.Offset.Ticks
                                            }
                                """;

                deserializeCode = $$"""
                                    bson => 
                                                {
                                                    var doc = bson.AsDocument;
                                                    var utcDateTime = doc["DateTime"].AsDateTime;
                                                    var offset = new System.TimeSpan(doc["Offset"].AsInt64);

                                                    // 1. Create a DateTimeOffset from the UTC time (its offset will be zero).
                                                    var utcTime = new System.DateTimeOffset(utcDateTime);

                                                    // 2. Convert it to the original offset.
                                                    var originalTime = utcTime.ToOffset(offset);

                                                    return {{name}}.From(originalTime);
                                                }
                                    """;
                break;

            default:
                serializeCode = "wrapper => new LiteDB.BsonValue(wrapper.Value)";
                deserializeCode =
                    $"bson => {name}.From(({argument})System.Convert.ChangeType(bson.RawValue, typeof({argument})))";
                break;
        }

        return $$"""
                 /// <summary>
                 /// Automatically registers the LiteDB BSON mapper for the {{name}} type.
                 /// </summary>
                 file static class {{name}}LiteDbInitializer
                 {
                     [System.Runtime.CompilerServices.ModuleInitializer]
                     internal static void Register()
                     {
                         LiteDB.BsonMapper.Global.RegisterType<{{name}}>(
                             serialize: {{serializeCode}},
                             deserialize: {{deserializeCode}}
                         );
                     }
                 }
                 """;
    }
}

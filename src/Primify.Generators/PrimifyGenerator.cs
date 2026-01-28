using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Primify.Generators;

[Generator]
public sealed class PrimifyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Filter to only attribute declarations
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        context.RegisterSourceOutput(provider, static (spc, model) => GenerateCode(spc, model!));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax;
    }

    private static PrimifyModel? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var node = (TypeDeclarationSyntax)context.Node;
        
        // Check if the type has the PrimifyAttribute
        var typeSymbol = context.SemanticModel.GetDeclaredSymbol(node);
        if (typeSymbol is null) return null;

        var hasPrimifyAttribute = typeSymbol.GetAttributes()
            .Any(a => a.AttributeClass?.Name == "PrimifyAttribute`1" || 
                      a.AttributeClass?.Name == "PrimifyAttribute");

        if (!hasPrimifyAttribute) return null;

        // Get the wrapped type from the attribute
        var attr = typeSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name.Contains("Primify") == true);

        var wrappedType = attr?.AttributeClass?.TypeArguments.FirstOrDefault()?.ToDisplayString() ?? "object";

        // Determine the keyword (class, struct, record class, record struct)
        var keyword = node switch
        {
            ClassDeclarationSyntax => "class",
            StructDeclarationSyntax => "struct",
            RecordDeclarationSyntax r => r.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword)
                ? "record class" : "record struct",
            _ => "class"
        };

        // Check for Normalize and Validate methods
        var hasNormalize = typeSymbol.GetMembers("Normalize").OfType<IMethodSymbol>().Any();
        var hasValidate = typeSymbol.GetMembers("Validate").OfType<IMethodSymbol>().Any();

        return new PrimifyModel(
            Namespace: typeSymbol.ContainingNamespace.ToDisplayString(),
            ClassName: typeSymbol.Name,
            Keyword: keyword,
            WrappedType: wrappedType,
            IsValueType: typeSymbol.IsValueType,
            IsRecord: node is RecordDeclarationSyntax,
            HasNormalize: hasNormalize,
            HasValidate: hasValidate,
            Location: node.Identifier.GetLocation()
        );
    }

    private static void GenerateCode(SourceProductionContext context, PrimifyModel model)
    {
        // Report diagnostics
        if (model.WrappedType == "object")
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.InvalidType,
                model.Location,
                model.ClassName));
        }

        if (!model.HasValidate)
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.ImplementValidate, 
                model.Location, 
                model.WrappedType));

        if (!model.HasNormalize)
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.ImplementNormalize, 
                model.Location, 
                model.WrappedType));

        context.ReportDiagnostic(Diagnostic.Create(
            Diagnostics.AddFactory, 
            model.Location, 
            model.ClassName));

        var source = PrimifyRenderer.Render(model);
        context.AddSource($"{model.Namespace}.{model.ClassName}.g.cs", source);
    }
}

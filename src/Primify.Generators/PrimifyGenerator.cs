using Flowgen;
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
        Flow.Create(context)
            .ForSyntax<PrimifyModel?>(static (node, _) => node is TypeDeclarationSyntax)
            .Select(static (ctx, _) => BuildModel(ctx))
            .Where(static model => model is not null)
            .Select(static model => model!)
            .Emit(static (spc, model) => GenerateCode(spc, model))
            .Build()
            .Initialize(context);
    }

    private static PrimifyModel? BuildModel(GeneratorSyntaxContext context)
    {
        if (context.Node is not TypeDeclarationSyntax node)
        {
            return null;
        }

        if (context.SemanticModel.GetDeclaredSymbol(node) is not INamedTypeSymbol typeSymbol)
        {
            return null;
        }

        var attr = typeSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "PrimifyAttribute`1" ||
                                 a.AttributeClass?.Name == "PrimifyAttribute");

        if (attr is null)
        {
            return null;
        }

        var wrappedType = attr.AttributeClass?.TypeArguments.FirstOrDefault()?.ToDisplayString()
            ?? "object";

        // Determine the keyword (class, struct, record class, record struct)
        var keyword = node switch
        {
            ClassDeclarationSyntax => "class",
            StructDeclarationSyntax => "struct",
            RecordDeclarationSyntax r => r.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword)
                ? "record class" : "record struct",
            _ => "class"
        };

        // Check for Normalize and Validate methods (private static, to avoid public API surface)
        var hasNormalize = typeSymbol.GetMembers("Normalize").OfType<IMethodSymbol>().Any(IsPrivateStaticNormalizer);
        var hasValidate = typeSymbol.GetMembers("Validate").OfType<IMethodSymbol>().Any(IsPrivateStaticVoidValidator);

        
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

    private static bool IsPrivateStaticNormalizer(IMethodSymbol method)
    {
        return method.DeclaredAccessibility == Accessibility.Private &&
               method.IsStatic &&
               method.Parameters.Length == 1;
    }

    private static bool IsPrivateStaticVoidValidator(IMethodSymbol method)
    {
        return method.DeclaredAccessibility == Accessibility.Private &&
               method.IsStatic &&
               method.ReturnType.SpecialType == SpecialType.System_Void &&
               method.Parameters.Length == 1;
    }
}

namespace Primify.Generators;

[Generator]
public sealed class PrimifyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        Flow.Create(context)
            .ForAttributeWithMetadataName<PrimifyModel>("Primify.Attributes.PrimifyAttribute`1")
            .Select((ctx, _) => ExtractModel(ctx))
            .Emit((spc, model) => GenerateCode(spc, model))
            .Build()
            .Initialize(context);
    }

    private static PrimifyModel ExtractModel(GeneratorAttributeSyntaxContext ctx)
    {
        var node = (TypeDeclarationSyntax)ctx.TargetNode;
        var symbol = ctx.TargetSymbol;

        if (symbol is not INamedTypeSymbol typeSymbol)
            throw new InvalidOperationException("Could not get symbol for type");

        var attr = typeSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name.Contains("Primify") == true);

        if (attr is null)
            throw new InvalidOperationException("No Primify attribute found");

        if (!node.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            throw new InvalidOperationException($"Type '{typeSymbol.Name}' must be partial");

        var wrappedType = attr.AttributeClass?.TypeArguments.FirstOrDefault()?.ToDisplayString() ?? "object";

        var keyword = node switch
        {
            ClassDeclarationSyntax => "class",
            StructDeclarationSyntax => "struct",
            RecordDeclarationSyntax r => r.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword)
                ? "record class" : "record struct",
            _ => "class"
        };

        return new PrimifyModel(
            Namespace: typeSymbol.ContainingNamespace.ToDisplayString(),
            ClassName: typeSymbol.Name,
            Keyword: keyword,
            WrappedType: wrappedType,
            IsValueType: typeSymbol.IsValueType,
            IsRecord: node is RecordDeclarationSyntax,
            HasNormalize: typeSymbol.GetMembers("Normalize").OfType<IMethodSymbol>().Any(),
            HasValidate: typeSymbol.GetMembers("Validate").OfType<IMethodSymbol>().Any(),
            Location: node.Identifier.GetLocation()
        );
    }

    private static void GenerateCode(SourceProductionContext spc, PrimifyModel model)
    {
        if (model.WrappedType == "object")
        {
            spc.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.InvalidType,
                model.Location,
                model.ClassName));
        }

        if (!model.HasValidate)
            spc.ReportDiagnostic(Diagnostic.Create(Diagnostics.ImplementValidate, model.Location, model.WrappedType));

        if (!model.HasNormalize)
            spc.ReportDiagnostic(Diagnostic.Create(Diagnostics.ImplementNormalize, model.Location, model.WrappedType));

        spc.ReportDiagnostic(Diagnostic.Create(Diagnostics.AddFactory, model.Location, model.ClassName));

        var source = PrimifyRenderer.Render(model);
        spc.AddSource($"{model.Namespace}.{model.ClassName}.g.cs", source);
    }
}

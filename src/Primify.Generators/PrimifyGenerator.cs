namespace Primify.Generators;

[Generator]
public class PrimifyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. Post-Init: Inject the attribute definition

        // 2. Start Pipeline
        Source.FromSyntax(context, (node, _) =>
            {
                // Fast syntactic check
                return node is TypeDeclarationSyntax { AttributeLists.Count: > 0 } t
                       && t.AttributeLists.Any(l => l.Attributes.Any(a => a.Name.ToString().Contains("Primify")));
            })

            // 3. Transform & Validate
            // Returns GenResult<PrimifyModel> which handles success/failure flow
            .SelectResult((ctx, ct) =>
            {
                var node = (TypeDeclarationSyntax)ctx.Node;

                if (ctx.SemanticModel.GetDeclaredSymbol(node) is not { } symbol)
                    return GenResult<PrimifyModel>.Fail(Diagnostic.Create(Diagnostics.TypeMustHaveAttributes,
                        node.GetLocation(), node.Identifier.Text));

                // Resolve Attribute
                var attr = symbol.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.Name.Contains("Primify") == true);
                if (attr is null)
                    return GenResult<PrimifyModel>.Fail(Diagnostic.Create(Diagnostics.Ignore, Location.None));

                // Validation: Must be partial
                if (!node.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                {
                    return GenResult<PrimifyModel>.Fail(Diagnostic.Create(Diagnostics.TypeMustBePartial,
                        node.Identifier.GetLocation(), node.Identifier.Text));
                }

                // Extract Data
                var wrappedType = attr.AttributeClass?.TypeArguments.FirstOrDefault()?.ToDisplayString() ?? "object";

                var keyword = node switch
                {
                    ClassDeclarationSyntax => "class",
                    StructDeclarationSyntax => "struct",
                    RecordDeclarationSyntax r => r.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword)
                        ? "record class"
                        : "record struct",
                    _ => "class"
                };

                var hasNormalize = symbol.GetMembers("Normalize").OfType<IMethodSymbol>().Any();
                var hasValidate = symbol.GetMembers("Validate").OfType<IMethodSymbol>().Any();

                return GenResult<PrimifyModel>.Success(new PrimifyModel(
                    Namespace: symbol.ContainingNamespace.ToDisplayString(),
                    ClassName: symbol.Name,
                    Keyword: keyword,
                    WrappedType: wrappedType,
                    IsValueType: symbol.IsValueType,
                    IsRecord: node is RecordDeclarationSyntax,
                    HasNormalize: hasNormalize,
                    HasValidate: hasValidate,
                    Location: node.Identifier.GetLocation()
                ));
            })

            // 4. Render Output
            // Automatically handles errors returned from SelectResult
            .Render((spc, model) =>
            {
                // Report Usage Hints (Info Diagnostics)
                if (!model.HasValidate)
                    spc.ReportDiagnostic(Diagnostic.Create(Diagnostics.ImplementValidate, model.Location,
                        model.WrappedType));

                if (!model.HasNormalize)
                    spc.ReportDiagnostic(Diagnostic.Create(Diagnostics.ImplementNormalize, model.Location,
                        model.WrappedType));

                spc.ReportDiagnostic(Diagnostic.Create(Diagnostics.AddFactory, model.Location, model.ClassName));

                // Generate Code
                var source = PrimifyRenderer.Render(model);
                spc.AddSource($"{model.Namespace}.{model.ClassName}.g.cs", source);
            });
    }
}

namespace Primify.Generators;

[Generator]
public class PrimifyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalGeneratorBuilder.Create()
            .StartSyntaxPipeline<PrimifyModel>()
            .WithPredicate((node, _) =>
            {
                if (node is not TypeDeclarationSyntax t) return false;
                if (t.AttributeLists.Count == 0) return false;

                // Basic string check to filter early
                return t.AttributeLists.Any(list =>
                    list.Attributes.Any(attr =>
                        attr.Name.ToString().IndexOf("Primify", StringComparison.OrdinalIgnoreCase) >= 0));
            })
            .WithTransform((ctx, _) =>
            {
                var node = (TypeDeclarationSyntax)ctx.Node;
                var symbol = ctx.SemanticModel.GetDeclaredSymbol(node);

                // If resolving failed, return error
                if (symbol is null)
                    return GenResult<PrimifyModel>.Fail(Diagnostic.Create(Diagnostics.TypeMustHaveAttributes,
                        node.GetLocation(), node.Identifier.Text));

                // Find attribute
                var primifyAttr = symbol.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.Name.Contains("Primify") == true);

                // If attribute is missing (false positive predicate), silently ignore.
                // Do not error out. Just skip.
                if (primifyAttr is null)
                {
                    return GenResult<PrimifyModel>.Fail(Diagnostic.Create(Diagnostics.Ignore, Location.None));
                }

                if (!node.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                {
                    return GenResult<PrimifyModel>.Fail(
                        Diagnostic.Create(Diagnostics.TypeMustBePartial, node.Identifier.GetLocation(),
                            node.Identifier.Text));
                }

                var wrapperArgument = primifyAttr.AttributeClass?.TypeArguments.FirstOrDefault()?.ToDisplayString() ??
                                      "object";

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

                return new PrimifyModel(
                    ns: symbol.ContainingNamespace.ToDisplayString(),
                    className: symbol.Name,
                    keyword: keyword,
                    wrapperArgument: wrapperArgument,
                    isValueType: symbol.IsValueType,
                    isRecord: node is RecordDeclarationSyntax,
                    hasNormalize: hasNormalize,
                    hasValidate: hasValidate,
                    location: node.Identifier.GetLocation()
                );
            })
            .WithOutput((spc, model, _) =>
            {
                // 1. Validate Hint
                if (!model.HasValidate)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.ImplementValidate,
                        model.Location,
                        model.WrapperArgument)); // {0}
                }

                // 2. Normalize Hint
                if (!model.HasNormalize)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.ImplementNormalize,
                        model.Location,
                        model.WrapperArgument)); // {0}
                }

                // 3. Factory Hint
                spc.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.AddFactory,
                    model.Location,
                    model.ClassName)); // {0}

                var sourceCode = PrimifyRenderer.Render(model);
                spc.AddSource($"{model.Namespace}.{model.ClassName}.g.cs", sourceCode);
            })
            .And()
            .Build()
            .Initialize(context);
    }
}

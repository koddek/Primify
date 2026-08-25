using Flowgen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace Primify.Generators;

[Generator]
public sealed class PrimifyGenerator : IIncrementalGenerator
{
    private const string AttributeMetadataName = "PrimifyAttribute`1";
    private const string AttributeNamespace = "Primify.Attributes";

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

        var attr = typeSymbol.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass is INamedTypeSymbol attributeClass &&
            attributeClass.MetadataName == AttributeMetadataName &&
            attributeClass.ContainingNamespace.ToDisplayString() == AttributeNamespace);

        if (attr is null)
        {
            return null;
        }

        var wrappedTypeSymbol = attr.AttributeClass?.TypeArguments.FirstOrDefault();
        var wrappedType = wrappedTypeSymbol?.ToDisplayString() ?? "object";

        // Determine the keyword (class, struct, record class, record struct)
        var keyword = node switch
        {
            ClassDeclarationSyntax => "class",
            StructDeclarationSyntax => "struct",
            RecordDeclarationSyntax r => r.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword)
                ? "record class" : "record struct",
            _ => "class"
        };

        // Normalize/Validate hooks are private static members declared in the user's partial part.
        var normalizeMembers = typeSymbol.GetMembers("Normalize").OfType<IMethodSymbol>().ToArray();
        var hasNormalize = normalizeMembers.Any(m => IsNormalizer(m, wrappedTypeSymbol));
        var invalidNormalize = !hasNormalize && normalizeMembers.Length > 0;

        var validateMembers = typeSymbol.GetMembers("Validate").OfType<IMethodSymbol>().ToArray();
        var hasValidate = validateMembers.Any(m => IsValidator(m, wrappedTypeSymbol));
        var invalidValidate = !hasValidate && validateMembers.Length > 0;

        return new PrimifyModel(
            Namespace: typeSymbol.ContainingNamespace.ToDisplayString(),
            ClassName: typeSymbol.Name,
            Keyword: keyword,
            WrappedType: wrappedType,
            WrappedTypeIsReferenceType: wrappedTypeSymbol?.IsReferenceType == true,
            IsValueType: typeSymbol.IsValueType,
            IsRecord: node is RecordDeclarationSyntax,
            HasNormalize: hasNormalize,
            HasValidate: hasValidate,
            InvalidNormalizeSignature: invalidNormalize,
            InvalidValidateSignature: invalidValidate,
            Location: DiagnosticLocation.From(node.Identifier.GetLocation()),
            ContainingTypes: GetContainingTypes(node)
        );
    }

    private static void GenerateCode(SourceProductionContext context, PrimifyModel model)
    {
        var location = model.Location.ToLocation();

        if (model.WrappedType == "object")
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.InvalidType,
                location,
                model.ClassName));

            return;
        }

        if (model.InvalidNormalizeSignature)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.InvalidNormalizeSignature,
                location,
                model.TypeName,
                model.WrappedType));
        }

        if (model.InvalidValidateSignature)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.InvalidValidateSignature,
                location,
                model.TypeName,
                model.WrappedType));
        }

        if (model.HasUnsupportedContainingType)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.ContainingTypeMustBePartial,
                location,
                model.TypeName));

            return;
        }

        var source = PrimifyRenderer.Render(model);
        context.AddSource(model.HintName, source);
    }

    private static EquatableArray<ContainingTypeModel> GetContainingTypes(TypeDeclarationSyntax node)
    {
        var containingTypes = new Stack<ContainingTypeModel>();

        for (var parent = node.Parent; parent is not null; parent = parent.Parent)
        {
            if (parent is not TypeDeclarationSyntax containingType)
            {
                continue;
            }

            containingTypes.Push(new ContainingTypeModel(
                Declaration: GetContainingTypeDeclaration(containingType),
                Name: containingType.Identifier.ValueText,
                TypeReferenceName: $"{containingType.Identifier.ValueText}{containingType.TypeParameterList}",
                IsPartial: containingType.Modifiers.Any(SyntaxKind.PartialKeyword)));
        }

        return containingTypes.Count == 0
            ? EquatableArray<ContainingTypeModel>.Empty
            : new EquatableArray<ContainingTypeModel>(containingTypes.ToArray());
    }

    private static string GetContainingTypeDeclaration(TypeDeclarationSyntax node)
    {
        var modifiers = string.Join(" ", node.Modifiers.Select(modifier => modifier.Text));
        var keyword = node switch
        {
            InterfaceDeclarationSyntax => "interface",
            ClassDeclarationSyntax => "class",
            StructDeclarationSyntax => "struct",
            RecordDeclarationSyntax record => record.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword)
                ? "record struct"
                : "record",
            _ => "class"
        };

        var constraints = node.ConstraintClauses.Count == 0
            ? string.Empty
            : $" {string.Join(" ", node.ConstraintClauses.Select(clause => clause.ToString()))}";

        return $"{modifiers} {keyword} {node.Identifier.ValueText}{node.TypeParameterList}{constraints}".Trim();
    }

    private static bool IsNormalizer(IMethodSymbol method, ITypeSymbol? wrappedType) =>
        wrappedType is not null &&
        method.DeclaredAccessibility == Accessibility.Private &&
        method.IsStatic &&
        method.Parameters.Length == 1 &&
        SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, wrappedType) &&
        SymbolEqualityComparer.Default.Equals(method.ReturnType, wrappedType);

    private static bool IsValidator(IMethodSymbol method, ITypeSymbol? wrappedType) =>
        wrappedType is not null &&
        method.DeclaredAccessibility == Accessibility.Private &&
        method.IsStatic &&
        method.Parameters.Length == 1 &&
        SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, wrappedType) &&
        method.ReturnType.SpecialType == SpecialType.System_Void;
}

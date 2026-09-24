using Flowgen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace Primify.Generators;

/// <summary>
/// Incremental source generator completing <c>[Primify&lt;T&gt;]</c>-annotated partial types into
/// validated primitive wrappers. For each annotated type it emits a generated part containing the
/// <c>Value</c> property, private constructor, <c>From</c>/<c>TryFrom</c> factories, conversion
/// operators, serializer attributes, equality members where the kind needs them, and a LiteDB
/// mapping initializer. Diagnostics: PRIT001 invalid wrapped type, PRIT002/PRIT003 hook signature
/// errors, PRIT004 non-partial containing type, PRIT005 unsupported declarations,
/// and PRIT006 reserved member conflicts.
/// </summary>
[Generator]
public sealed class PrimifyGenerator : IIncrementalGenerator
{
    private const string AttributeMetadataName = "PrimifyAttribute`1";
    private const string AttributeNamespace = "Primify.Attributes";

    private static readonly string[] CommonReservedMembers =
        ["Value", "From", "TryFrom", "op_Implicit", "op_Explicit"];

    private static readonly string[] EqualityReservedMembers =
        ["ToString", "Equals", "GetHashCode", "op_Equality", "op_Inequality"];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        Flow.Create()
            .ForSyntax(static (node, _) => node is TypeDeclarationSyntax declaration && declaration.AttributeLists.Count > 0)
            .Select(static (context, _) => BuildModel(context))
            .Where(static model => model is not null)
            .Select(static model => model!)
            .Emit(static (spc, model) => GenerateCode(spc, model))
            .Build()
            .Initialize(context);
    }

    private static bool IsPrimifyAttribute(AttributeData attribute) =>
        attribute.AttributeClass is INamedTypeSymbol attributeClass &&
        attributeClass.MetadataName == AttributeMetadataName &&
        attributeClass.ContainingNamespace.ToDisplayString() == AttributeNamespace;

    private static PrimifyModel? BuildModel(GeneratorSyntaxContext context)
    {
        if (context.Node is not TypeDeclarationSyntax node ||
            context.SemanticModel.GetDeclaredSymbol(node) is not INamedTypeSymbol typeSymbol)
        {
            return null;
        }

        var attr = typeSymbol.GetAttributes().FirstOrDefault(IsPrimifyAttribute);
        if (attr is null)
        {
            return null;
        }

        var wrappedTypeSymbol = attr.AttributeClass?.TypeArguments.FirstOrDefault();
        var wrappedType = wrappedTypeSymbol?.ToDisplayString() ?? "object";
        var containingTypes = GetContainingTypes(node);

        var keyword = node switch
        {
            ClassDeclarationSyntax => "class",
            StructDeclarationSyntax => "struct",
            RecordDeclarationSyntax record => record.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword)
                ? "record struct"
                : "record class",
            _ => "class"
        };

        var normalizeMembers = typeSymbol.GetMembers("Normalize").ToArray();
        var normalizeMethods = normalizeMembers.OfType<IMethodSymbol>().ToArray();
        var hasNormalize = normalizeMethods.Any(m => IsNormalizer(m, wrappedTypeSymbol));
        var invalidNormalize = normalizeMembers.Any(member =>
            member is not IMethodSymbol method || !IsNormalizer(method, wrappedTypeSymbol));

        var validateMembers = typeSymbol.GetMembers("Validate").ToArray();
        var validateMethods = validateMembers.OfType<IMethodSymbol>().ToArray();
        var hasValidate = validateMethods.Any(m => IsValidator(m, wrappedTypeSymbol));
        var invalidValidate = validateMembers.Any(member =>
            member is not IMethodSymbol method || !IsValidator(method, wrappedTypeSymbol));

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
            UnsupportedDeclarationReason: GetUnsupportedDeclarationReason(node, typeSymbol, containingTypes),
            ReservedMemberConflict: GetReservedMemberConflict(typeSymbol, wrappedTypeSymbol),
            Location: DiagnosticLocation.From(node.Identifier.GetLocation()),
            ContainingTypes: containingTypes
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

        if (model.UnsupportedDeclarationReason is { } unsupportedReason)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.UnsupportedTypeDeclaration,
                location,
                model.TypeName,
                unsupportedReason));

            return;
        }

        if (model.ReservedMemberConflict is { } reservedMember)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.ReservedMemberConflict,
                location,
                model.TypeName,
                reservedMember));

            return;
        }

        var hasInvalidHook = false;
        if (model.InvalidNormalizeSignature)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.InvalidNormalizeSignature,
                location,
                model.TypeName,
                model.WrappedType));
            hasInvalidHook = true;
        }

        if (model.InvalidValidateSignature)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.InvalidValidateSignature,
                location,
                model.TypeName,
                model.WrappedType));
            hasInvalidHook = true;
        }

        if (hasInvalidHook)
        {
            return;
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

    private static string? GetReservedMemberConflict(INamedTypeSymbol typeSymbol, ITypeSymbol? wrappedType)
    {
        if (wrappedType is not null && typeSymbol.InstanceConstructors.Any(constructor =>
                constructor.DeclaringSyntaxReferences.Length > 0 &&
                constructor.Parameters.Length == 1 &&
                SymbolEqualityComparer.Default.Equals(constructor.Parameters[0].Type, wrappedType)))
        {
            return "a constructor";
        }

        foreach (var memberName in CommonReservedMembers)
        {
            if (HasSourceMember(typeSymbol, memberName))
            {
                return memberName;
            }
        }

        if (typeSymbol.IsRecord)
        {
            return null;
        }

        foreach (var memberName in EqualityReservedMembers)
        {
            if (HasSourceMember(typeSymbol, memberName))
            {
                return memberName;
            }
        }

        return null;
    }

    private static bool HasSourceMember(INamedTypeSymbol typeSymbol, string name) =>
        typeSymbol.GetMembers(name).Any(member => member.DeclaringSyntaxReferences.Length > 0);

    private static string? GetUnsupportedDeclarationReason(
        TypeDeclarationSyntax node,
        INamedTypeSymbol typeSymbol,
        EquatableArray<ContainingTypeModel> containingTypes)
    {
        if (typeSymbol.Arity > 0 || containingTypes.Items?.Any(type => type.IsGeneric) == true)
        {
            return "generic wrappers and wrappers nested in generic types are not supported";
        }

        if (typeSymbol.IsStatic)
        {
            return "static types are not supported";
        }

        if (typeSymbol.IsAbstract)
        {
            return "abstract types are not supported";
        }

        if (node.Modifiers.Any(SyntaxKind.FileKeyword) || HasFileLocalContainingType(typeSymbol))
        {
            return "file-local types are not supported";
        }

        if (node.Modifiers.Any(SyntaxKind.RefKeyword))
        {
            return "ref-like types are not supported";
        }

        if (!node.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return "the wrapper type must be partial";
        }

        if (HasInaccessibleContainingType(typeSymbol))
        {
            return "the wrapper and every containing type must be accessible from generated namespace-level code";
        }

        return null;
    }

    private static bool HasFileLocalContainingType(INamedTypeSymbol typeSymbol)
    {
        for (var containingType = typeSymbol.ContainingType; containingType is not null; containingType = containingType.ContainingType)
        {
            if (containingType.DeclaringSyntaxReferences.Any(reference =>
                    reference.GetSyntax() is TypeDeclarationSyntax node &&
                    node.Modifiers.Any(SyntaxKind.FileKeyword)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasInaccessibleContainingType(INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.ContainingType is not null && IsInaccessible(typeSymbol))
        {
            return true;
        }

        for (var containingType = typeSymbol.ContainingType; containingType is not null; containingType = containingType.ContainingType)
        {
            if (IsInaccessible(containingType))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInaccessible(INamedTypeSymbol typeSymbol) =>
        typeSymbol.DeclaredAccessibility is Accessibility.Private or
            Accessibility.Protected or
            Accessibility.ProtectedAndInternal or
            Accessibility.ProtectedOrInternal;

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
                IsPartial: containingType.Modifiers.Any(SyntaxKind.PartialKeyword),
                IsGeneric: containingType.TypeParameterList is { Parameters.Count: > 0 }));
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
        wrappedType is not null && IsHookShape(method, wrappedType) &&
        SymbolEqualityComparer.Default.Equals(method.ReturnType, wrappedType);

    private static bool IsValidator(IMethodSymbol method, ITypeSymbol? wrappedType) =>
        wrappedType is not null && IsHookShape(method, wrappedType) &&
        method.ReturnType.SpecialType == SpecialType.System_Void;

    private static bool IsHookShape(IMethodSymbol method, ITypeSymbol wrappedType) =>
        method.MethodKind == MethodKind.Ordinary &&
        method.DeclaredAccessibility == Accessibility.Private &&
        method.IsStatic &&
        !method.IsAsync &&
        method.Arity == 0 &&
        method.RefKind == RefKind.None &&
        method.Parameters.Length == 1 &&
        method.Parameters[0].RefKind == RefKind.None &&
        SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, wrappedType);
}

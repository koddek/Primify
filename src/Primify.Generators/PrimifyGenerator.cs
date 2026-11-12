using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace Primify.Generators;

/// <summary>
/// Incremental source generator completing <c>[Primify&lt;T&gt;]</c>-annotated partial types into
/// validated primitive wrappers. For each annotated type it emits a generated part containing the
/// <c>Value</c> property, private constructor, <c>From</c>/<c>TryFrom</c> factories, conversion
/// operators, serializer attributes, and equality members where the kind needs them. When the consumer
/// references LiteDB or Newtonsoft.Json, it additionally emits one shared adapter source per compilation
/// that converts wrappers to the serializer's native types at the point of serialization, so the Primify
/// package itself carries no serializer dependency. Diagnostics: PRIT001 invalid wrapped type,
/// PRIT002/PRIT003 hook signature errors, PRIT004 non-partial containing type, PRIT005 unsupported
/// declarations, PRIT006 reserved member conflicts, and PRIT007 LiteDB referenced without a built-in bridge.
/// </summary>
[Generator]
public sealed class PrimifyGenerator : IIncrementalGenerator
{
    private const string AttributeMetadataName = "PrimifyAttribute`1";
    private const string AttributeNamespace = "Primify.Attributes";

    private static readonly ConditionalWeakTable<SyntaxTree, string[]> PrimifyAliasCache = new();

    private static readonly string[] CommonReservedMembers =
        ["Value", "From", "TryFrom", "op_Implicit", "op_Explicit"];

    private static readonly string[] EqualityReservedMembers =
        ["ToString", "Equals", "GetHashCode", "op_Equality", "op_Inequality"];

    // Every non-special type the generator emits is global::-qualified through this format. Special types
    // (int, string, ...) stay keywords — they cannot be shadowed — and nullability annotations are kept so
    // a wrapped string? keeps its annotation in generated source.
    private static readonly SymbolDisplayFormat QualifiedTypeFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
            SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
            SymbolDisplayMiscellaneousOptions.ExpandNullable |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    /// <summary>
    /// Registers the incremental pipeline. Every attributed type declaration is modeled once; the modeled
    /// list is collected once for the shared serializer adapters and emitted per wrapper otherwise. Building
    /// the model a single time keeps the semantic analysis (attribute lookup, member scans) cheap on every
    /// re-keystroke.
    /// </summary>
    /// <param name="context">The Roslyn initialization context the pipeline is attached to.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var models = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is TypeDeclarationSyntax declaration && HasPotentialPrimifyAttribute(declaration),
                static (syntaxContext, _) => BuildModel(syntaxContext))
            .Where(static model => model is not null)
            .Select(static (model, _) => (PrimifyModel)model!);

        context.RegisterSourceOutput(models, static (spc, model) => GenerateCode(spc, model));

        // Serializer adapter emission: one shared source per consumer compilation, emitted only when the
        // consumer references the matching serializer package (LiteDB, Newtonsoft.Json, Newtonsoft.Json.Bson).
        context.RegisterSourceOutput(
            models.Collect().Combine(context.CompilationProvider),
            static (spc, pair) => GenerateAdapters(spc, pair.Left, pair.Right));
    }

    private static bool IsPrimifyAttribute(AttributeData attribute) =>
        attribute.AttributeClass is INamedTypeSymbol attributeClass &&
        attributeClass.MetadataName == AttributeMetadataName &&
        attributeClass.ContainingNamespace.ToDisplayString() == AttributeNamespace;

    // Cheap syntactic gate that runs before semantic binding. Accepts:
    //   - attributes spelled [Primify] / [PrimifyAttribute] (qualified or not),
    //   - any generic attribute — covers [Primify<int>] and unbound-generic aliases like [Q<int>],
    //   - attributes spelled with an identifier that this file declares as a using-alias (e.g. [P] for
    //     [Primify<int>]); aliases are collected once per syntax tree and cached.
    // The semantic IsPrimifyAttribute check in BuildModel stays the authority, so this only ever
    // narrows work, never changes which types are accepted.
    private static bool HasPotentialPrimifyAttribute(TypeDeclarationSyntax declaration)
    {
        foreach (var attributeList in declaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                if (attribute.Name is GenericNameSyntax)
                {
                    return true;
                }

                var name = SimpleIdentifier(attribute.Name);
                if (name is "Primify" or "PrimifyAttribute" || IsPrimifyAlias(name, declaration.SyntaxTree))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsPrimifyAlias(string? name, SyntaxTree tree)
    {
        if (name is null)
        {
            return false;
        }

        foreach (var alias in PrimifyAliases(tree))
        {
            if (string.Equals(alias, name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    // Collects the identifiers of using-aliases in this file whose aliased type could be PrimifyAttribute<T>
    // or [Primify<T>]'s attribute. Cheap string scan; cached per tree.
    private static string[] PrimifyAliases(SyntaxTree tree) =>
        PrimifyAliasCache.GetValue(tree, static syntaxTree =>
        {
            var aliases = new List<string>();
            foreach (var usingDirective in syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<UsingDirectiveSyntax>())
            {
                var aliasName = usingDirective.Alias?.Name?.ToString();
                if (aliasName is not { Length: > 0 } || usingDirective.Name is null)
                {
                    continue;
                }

                var text = usingDirective.Name.ToString();
                if (text.IndexOf("PrimifyAttribute", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    text.IndexOf("Primify<", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    aliases.Add(aliasName);
                }
            }

            return aliases.ToArray();
        });

    private static string? SimpleIdentifier(NameSyntax name) => name switch
    {
        IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
        GenericNameSyntax generic => generic.Identifier.ValueText,
        QualifiedNameSyntax qualified => SimpleIdentifier(qualified.Right),
        AliasQualifiedNameSyntax alias => SimpleIdentifier(alias.Name),
        _ => null
    };

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
        var wrappedType = wrappedTypeSymbol?.ToDisplayString();
        var wrappedTypeQualified = wrappedTypeSymbol?.ToDisplayString(QualifiedTypeFormat);
        var wrapsNullableValueType = wrappedTypeSymbol is INamedTypeSymbol nullableSymbol &&
            nullableSymbol.IsValueType &&
            nullableSymbol.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T;
        var underlyingWrappedType = wrapsNullableValueType
            ? ((INamedTypeSymbol)wrappedTypeSymbol!).TypeArguments[0].ToDisplayString(QualifiedTypeFormat)
            : wrappedTypeQualified ?? string.Empty;
        var containingTypes = GetContainingTypes(node);
        var typeName = containingTypes.IsEmpty
            ? typeSymbol.Name
            : $"{containingTypes.Join(static type => type.Name)}.{typeSymbol.Name}";

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

        var compilation = context.SemanticModel.Compilation;

        var rejections = new List<Rejection>();
        if (wrappedType is null)
        {
            rejections.Add(new InvalidTypeRejection(typeName));
        }

        var unsupportedReason = GetUnsupportedDeclarationReason(node, typeSymbol, containingTypes);
        if (unsupportedReason is { } reason)
        {
            rejections.Add(new UnsupportedDeclarationRejection(typeName, reason));
        }

        var reservedMember = GetReservedMemberConflict(typeSymbol, wrappedTypeSymbol);
        if (reservedMember is { } member)
        {
            rejections.Add(new ReservedMemberConflictRejection(typeName, member));
        }

        if (invalidNormalize)
        {
            rejections.Add(new InvalidNormalizeRejection(typeName, wrappedType ?? string.Empty));
        }

        if (invalidValidate)
        {
            rejections.Add(new InvalidValidateRejection(typeName, wrappedType ?? string.Empty));
        }

        if (containingTypes.Any(static type => !type.IsPartial))
        {
            rejections.Add(new ContainingTypeNotPartialRejection(typeName));
        }

        return new PrimifyModel(
            Namespace: typeSymbol.ContainingNamespace.ToDisplayString(),
            ClassName: typeSymbol.Name,
            Keyword: keyword,
            WrappedType: wrappedType,
            WrappedTypeQualified: wrappedTypeQualified,
            WrappedTypeIsReferenceType: wrappedTypeSymbol?.IsReferenceType == true,
            IsValueType: typeSymbol.IsValueType,
            IsRecord: node is RecordDeclarationSyntax,
            EqualityStrategy: ResolveEqualityStrategy(wrappedTypeSymbol),
            HasNormalize: hasNormalize,
            HasValidate: hasValidate,
            Rejections: new EquatableArray<Rejection>([.. rejections]),
            Location: DiagnosticLocation.From(node.Identifier.GetLocation()),
            ContainingTypes: containingTypes,
            LiteDbAvailable: compilation.GetTypeByMetadataName("LiteDB.BsonMapper") is not null,
            LiteDbStorage: LiteDbStorage.Classify(wrappedTypeSymbol),
            WrapsNullableValueType: wrapsNullableValueType,
            UnderlyingWrappedType: underlyingWrappedType,
            NewtonsoftAvailable: compilation.GetTypeByMetadataName("Newtonsoft.Json.JsonConverter") is not null,
            NewtonsoftBsonAvailable: compilation.GetTypeByMetadataName("Newtonsoft.Json.Bson.BsonDataWriter") is not null
        );
    }

    private static void GenerateCode(SourceProductionContext context, PrimifyModel model)
    {
        var location = model.Location.ToLocation();

        if (!model.Rejections.IsEmpty)
        {
            foreach (var rejection in model.Rejections)
            {
                context.ReportDiagnostic(rejection.Create(location));
            }

            return;
        }

        if (model is { LiteDbAvailable: true, LiteDbStorage: LiteDbStorageKind.None })
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.NoLiteDbBridge,
                location,
                model.TypeName,
                model.WrappedType));
        }

        var source = PrimifyRenderer.Render(model);
        context.AddSource(model.HintName, source);
    }

    private static void GenerateAdapters(
        SourceProductionContext context,
        ImmutableArray<PrimifyModel> models,
        Compilation compilation)
    {
        if (models.Length == 0)
        {
            return;
        }

        if (compilation.GetTypeByMetadataName("Newtonsoft.Json.JsonConverter") is not null)
        {
            var bsonAvailable = compilation.GetTypeByMetadataName("Newtonsoft.Json.Bson.BsonDataWriter") is not null;
            context.AddSource(
                "Primify.NewtonsoftJsonConverter.g.cs",
                SerializerAdapterRenderer.RenderNewtonsoftConverter(bsonAvailable));
        }

        if (compilation.GetTypeByMetadataName("LiteDB.BsonMapper") is not null)
        {
            var registrable = models
                .Where(static model => model.IsGeneratable && model.LiteDbStorage != LiteDbStorageKind.None)
                .ToArray();

            if (registrable.Length > 0)
            {
                context.AddSource(
                    "Primify.LiteDbRegistration.g.cs",
                    SerializerAdapterRenderer.RenderLiteDbRegistration(registrable));
            }
        }
    }

    private static string? GetReservedMemberConflict(INamedTypeSymbol typeSymbol, ITypeSymbol? wrappedType)
    {
        if (wrappedType is not null && typeSymbol.InstanceConstructors.Any(constructor =>
                constructor.DeclaringSyntaxReferences.Length > 0 &&
                constructor.Parameters.Length == 1 &&
                SymbolEqualityComparer.Default.Equals(constructor.Parameters[0].Type, wrappedType)))
        {
            return $"a constructor with a single parameter of type '{wrappedType.ToDisplayString()}'";
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
        // Generated code lives at namespace level, so a nested wrapper must itself be reachable from there
        // and so must every type that encloses it.
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

    private static EqualityStrategy ResolveEqualityStrategy(ITypeSymbol? wrappedType)
    {
        if (wrappedType?.SpecialType == SpecialType.System_String)
        {
            return EqualityStrategy.OrdinalString;
        }

        return HasReflexiveOperatorEquality(wrappedType)
            ? EqualityStrategy.Operator
            : EqualityStrategy.EqualityComparer;
    }

    /// <summary>
    /// Decides from the symbol whether the type's <c>==</c> is reflexive. Floating-point types are
    /// deliberately excluded because <c>NaN != NaN</c>, so those wrappers must compare through
    /// <c>EqualityComparer&lt;T&gt;.Default</c> to stay reflexive. Anything unrecognised takes the same
    /// fallback, which stays correct even for types with an inconsistent <c>==</c> overload.
    /// </summary>
    private static bool HasReflexiveOperatorEquality(ITypeSymbol? wrappedType) =>
        wrappedType?.SpecialType switch
        {
            SpecialType.System_Boolean or
            SpecialType.System_Char or
            SpecialType.System_SByte or
            SpecialType.System_Byte or
            SpecialType.System_Int16 or
            SpecialType.System_UInt16 or
            SpecialType.System_Int32 or
            SpecialType.System_UInt32 or
            SpecialType.System_Int64 or
            SpecialType.System_UInt64 or
            SpecialType.System_IntPtr or
            SpecialType.System_UIntPtr or
            SpecialType.System_Decimal or
            SpecialType.System_DateTime => true,
            _ => IsSystemType(wrappedType, "Guid")
                || IsSystemType(wrappedType, "DateTimeOffset")
                || IsSystemType(wrappedType, "DateOnly")
                || IsSystemType(wrappedType, "TimeOnly")
                || IsSystemType(wrappedType, "TimeSpan")
        };

    /// <summary>
    /// Identifies a framework type by symbol identity rather than by the spelling the user wrote, so
    /// aliases and <c>global::</c> qualification do not change the generated equality members.
    /// </summary>
    private static bool IsSystemType(ITypeSymbol? symbol, string metadataName) =>
        symbol is INamedTypeSymbol named &&
        named.MetadataName == metadataName &&
        named.ContainingNamespace is { IsGlobalNamespace: false } containingNamespace &&
        containingNamespace.ToDisplayString() == "System";

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

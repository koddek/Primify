using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Primify.Generator;

internal sealed class TypeDeclarationWithDiagnostics(
    TypeDeclarationSyntax typeDeclaration,
    bool reportAttributeFound,
    ImmutableArray<Diagnostic> diagnostics = default
)
{
    public TypeDeclarationSyntax TypeDeclaration { get; } = typeDeclaration;
    public bool ReportAttributeFound { get; } = reportAttributeFound;
    public ImmutableArray<Diagnostic> Diagnostics { get; } = diagnostics;

    public static implicit operator (TypeDeclarationSyntax, bool)
        (TypeDeclarationWithDiagnostics value) => (value.TypeDeclaration, value.ReportAttributeFound);
}

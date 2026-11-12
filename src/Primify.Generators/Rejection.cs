namespace Primify.Generators;

using Microsoft.CodeAnalysis;

/// <summary>
/// Why a wrapper cannot be generated. This is a closed hierarchy, so every rejection switch in the
/// generator is checked exhaustively: adding a new case fails the build until the generator handles
/// it. Each case is an equatable record, keeping the model cache-friendly. A rejection is a permanent
/// "do not generate" decision — diagnostics are reported once per declaration.
/// </summary>
internal closed record class Rejection
{
    /// <summary>The descriptor used to report the rejection.</summary>
    public abstract DiagnosticDescriptor Descriptor { get; }

    /// <summary>The arguments interpolated into <see cref="Descriptor"/>'s message.</summary>
    public abstract object?[] MessageArgs { get; }

    /// <summary>Creates the diagnostic for this rejection at the given location.</summary>
    public Diagnostic Create(Location location) => Diagnostic.Create(Descriptor, location, MessageArgs);
}

/// <summary>PRIT001 — the wrapped type argument could not be resolved.</summary>
internal sealed record InvalidTypeRejection(string TypeName) : Rejection
{
    /// <inheritdoc/>
    public override DiagnosticDescriptor Descriptor => Diagnostics.InvalidType;

    /// <inheritdoc/>
    public override object?[] MessageArgs => [TypeName];
}

/// <summary>PRIT002 — the Normalize hook exists but does not match the required signature.</summary>
internal sealed record InvalidNormalizeRejection(string TypeName, string WrappedType) : Rejection
{
    /// <inheritdoc/>
    public override DiagnosticDescriptor Descriptor => Diagnostics.InvalidNormalizeSignature;

    /// <inheritdoc/>
    public override object?[] MessageArgs => [TypeName, WrappedType];
}

/// <summary>PRIT003 — the Validate hook exists but does not match the required signature.</summary>
internal sealed record InvalidValidateRejection(string TypeName, string WrappedType) : Rejection
{
    /// <inheritdoc/>
    public override DiagnosticDescriptor Descriptor => Diagnostics.InvalidValidateSignature;

    /// <inheritdoc/>
    public override object?[] MessageArgs => [TypeName, WrappedType];
}

/// <summary>PRIT004 — a type containing the wrapper is not partial.</summary>
internal sealed record ContainingTypeNotPartialRejection(string TypeName) : Rejection
{
    /// <inheritdoc/>
    public override DiagnosticDescriptor Descriptor => Diagnostics.ContainingTypeMustBePartial;

    /// <inheritdoc/>
    public override object?[] MessageArgs => [TypeName];
}

/// <summary>PRIT005 — the declaration shape is not supported.</summary>
internal sealed record UnsupportedDeclarationRejection(string TypeName, string Reason) : Rejection
{
    /// <inheritdoc/>
    public override DiagnosticDescriptor Descriptor => Diagnostics.UnsupportedTypeDeclaration;

    /// <inheritdoc/>
    public override object?[] MessageArgs => [TypeName, Reason];
}

/// <summary>PRIT006 — the user declaration already uses a member Primify owns.</summary>
internal sealed record ReservedMemberConflictRejection(string TypeName, string Member) : Rejection
{
    /// <inheritdoc/>
    public override DiagnosticDescriptor Descriptor => Diagnostics.ReservedMemberConflict;

    /// <inheritdoc/>
    public override object?[] MessageArgs => [TypeName, Member];
}

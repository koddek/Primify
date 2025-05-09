// This file contains code snippets intended to trigger specific diagnostics
// from the ValueWrapperGenerator. These are not runnable tests in the traditional
// sense but are for verifying diagnostic output during compilation.

using System;
using Primify.Attributes;

namespace Primify.Tests.DiagnosticTriggers;

// --- Trigger for VWG002: Type not partial ---
// [Primify<int>]
// public record class NonPartialId // Error: VWG002 The type 'NonPartialId' decorated with 'PrimifyAttribute' must be declared with the 'partial' keyword
// {
// }

// --- Trigger for VWG003: Type not a record class or record struct ---
// [Primify<int>]
// public partial class NonRecordId // Error: VWG003 The type 'NonRecordId' decorated with 'PrimifyAttribute' must be a record class or readonly record struct
// {
//     public int Value { get; }
//     private NonRecordId(int value) => Value = value;
//     public static NonRecordId From(int value) => new(value);
// }

// --- Trigger for VWG004: Struct not readonly ---
// [Primify<int>]
// public partial record struct NonReadonlyStructId // Error: VWG004 The struct type 'NonReadonlyStructId' decorated with 'PrimifyAttribute' must be declared with the 'readonly' modifier
// {
// }

// --- Triggers for VWG005: Invalid Attribute Usage ---

// VWG005: Unsupported primitive type
// public class SomeUnsupportedType {}
// [Primify<SomeUnsupportedType>]
// public partial record class UnsupportedPrimitiveId // Error: VWG005 PrimifyAttribute on type 'UnsupportedPrimitiveId' has invalid type arguments or configuration: The primitive type 'Primify.Tests.DiagnosticTriggers.SomeUnsupportedType' is not a supported primitive or system type
// {
// }

// VWG005: Nullable primitive type (value type)
// [Primify<int?>]
// public partial record class NullableIntId // Error: VWG005 PrimifyAttribute on type 'NullableIntId' has invalid type arguments or configuration: The primitive type argument 'int?' cannot be a nullable reference or value type
// {
// }

// VWG005: Nullable primitive type (reference type - though string is special, let's use a different one if possible, or illustrate with string?)
// Note: string? is allowed by C# but the generator might restrict it.
// The current check is `primitiveTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated`
// [Primify<string?>] // This might depend on project's nullable context. If string? is seen as annotated.
// public partial record class NullableStringId // Potential Error: VWG005 ... primitive type argument 'string?' cannot be a nullable ...
// {
// }


// --- Trigger for VWG007: InfoPartialMethodAvailable (for Normalize and Validate) ---
// This type should generate VWG007 for Normalize and VWG007 for Validate
[Primify<double>]
public partial record class NoCustomLogicValue
{
    // No Normalize or Validate methods defined by user
}

// --- Trigger for VWG009: Predefined property not partial ---
// [Primify<string>]
// public partial record class NonPartialPredefinedProperty
// {
//     [PredefinedValue("test")]
//     public static string MyValue { get; } // Error: VWG009 Property 'MyValue' decorated with PredefinedValueAttribute must be declared with the 'partial' keyword
// }

// --- Trigger for VWG005: PredefinedValue type mismatch ---
// (This was already explored with TimeOnly, but to formally list it)
// [Primify<System.TimeOnly>]
// public partial record class PredefinedTypeMismatch
// {
//     [PredefinedValue("this is not a TimeOnly")] // Error VWG005: ... Value type 'string' for property 'BadTime' does not match ...
//     public static partial PredefinedTypeMismatch BadTime { get; }
// }

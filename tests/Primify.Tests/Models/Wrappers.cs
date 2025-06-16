namespace Primify.Generator.Tests.Models;

[Primify<int>]
public sealed partial class Class3;

[Primify<string>]
public partial record class RecordClass4;

[Primify<int>]
public sealed partial record class RecordClass5;

[Primify<double>]
public readonly partial struct Struct3;

[Primify<string>]
public partial record struct RecordStruct4;

[Primify<double>]
public readonly partial record struct RecordStruct5;

// Class wrappers

[Primify<int>]
public partial class ClassId;

[Primify<int>]
public sealed partial class SealedClassId;

[Primify<int>]
public partial record class RecordClassId;

[Primify<int>]
public sealed partial record class SealedRecordClassId;

// Struct wrappers

[Primify<int>]
public partial struct StructId;

[Primify<int>]
public readonly partial struct ReadonlyStructId;

[Primify<int>]
public partial record struct RecordStructId;

[Primify<int>]
public readonly partial record struct ReadonlyRecordStructId;

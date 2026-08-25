using Primify.Attributes;

namespace Primify.Generator.Tests.Types;

public partial interface IUsers
{
    public sealed partial record Male
    {
        public MaleId Id { get; set; }

        [Primify<string>]
        public readonly partial record struct MaleId;
    }

    public sealed partial record Female
    {
        public FemaleId Id { get; set; }

        [Primify<string>]
        public readonly partial record struct FemaleId;
    }
}

// Nested struct containing a Primify type
public partial struct NestedStructContainer
{
    public InnerStructId Id { get; set; }

    [Primify<int>]
    public partial struct InnerStructId;
}

// Nested record class containing a Primify type
public partial record class NestedRecordClassContainer
{
    public InnerRecordClassId Id { get; set; }

    [Primify<string>]
    public sealed partial record class InnerRecordClassId;
}

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

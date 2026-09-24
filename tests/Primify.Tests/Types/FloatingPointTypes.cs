using Primify.Attributes;

namespace Primify.Generator.Tests.Types;

[Primify<float>]
public partial struct FloatValueWrapper;

[Primify<double>]
public partial struct DoubleValueWrapper;

[Primify<Half>]
public partial struct HalfValueWrapper;

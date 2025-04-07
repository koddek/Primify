using System;
using Primify.Attributes;

namespace Primify.Tests.Models;

[Primify<Guid>]
public partial record class ItemId
{
    [PredefinedValue("00000000-0000-0000-0000-000000000000")]
    public static partial ItemId Empty { get; }

    // [PredefinedValue("99999999-9999-9999-9999-999999999999")]
    [PredefinedValue("ffffffff-ffff-ffff-ffff-ffffffffffff")]
    public static partial ItemId Undefined { get; }
}

using System;

namespace NuGettier.Upm;

[GuidFormatterType(GuidFormat.None)]
public class NullGuidFormatter : IGuidFormatter
{
    public virtual Guid Apply(Guid guid) => guid;
}

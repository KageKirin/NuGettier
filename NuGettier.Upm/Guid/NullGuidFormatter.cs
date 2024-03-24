using System;

namespace NuGettier.Upm;

public class NullGuidFormatter : IGuidFormatter
{
    public virtual Guid Apply(Guid guid) => guid;
}

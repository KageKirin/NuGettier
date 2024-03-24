using System;

namespace NuGettier.Upm;

public class Rfc4122GuidFormatter : IGuidFormatter
{
    public virtual Guid Apply(Guid guid) => guid.ToRfc4122();
}

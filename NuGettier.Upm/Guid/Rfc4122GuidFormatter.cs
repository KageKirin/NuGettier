using System;

namespace NuGettier.Upm;

[GuidFormatterType(GuidFormat.Rfc4122)]
public class Rfc4122GuidFormatter : IGuidFormatter
{
    public virtual Guid Apply(Guid guid) => guid.ToRfc4122();
}

using System;

namespace NuGettier.Upm;

[GuidFormatterType(GuidFormat.Unity)]
public class UnityGuidFormatter : IGuidFormatter
{
    public virtual Guid Apply(Guid guid) => guid.ToUnityGUID();
}

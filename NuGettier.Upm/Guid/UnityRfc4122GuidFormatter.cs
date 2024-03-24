using System;

namespace NuGettier.Upm;

[GuidFormatterType(GuidFormat.Unity_Rfc4122)]
public class UnityRfc4122GuidFormatter : IGuidFormatter
{
    public virtual Guid Apply(Guid guid) => guid.ToUnityGUID().ToRfc4122();
}

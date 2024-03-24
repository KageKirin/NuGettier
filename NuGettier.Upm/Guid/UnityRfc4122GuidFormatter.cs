using System;

namespace NuGettier.Upm;

public class UnityRfc4122GuidFormatter : IGuidFormatter
{
    public virtual Guid Apply(Guid guid) => guid.ToUnityGUID().ToRfc4122();
}

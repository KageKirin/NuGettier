using System;

namespace NuGettier.Upm;

public class UnityGuidFormatter : IGuidFormatter
{
    public virtual Guid Apply(Guid guid) => guid.ToUnityGUID();
}

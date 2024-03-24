using System;

namespace NuGettier.Upm;

public interface IGuidFormatter
{
    Guid Apply(Guid guid);
}

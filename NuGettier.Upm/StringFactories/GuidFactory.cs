using System;

namespace NuGettier.Upm;

public interface IGuidFactory
{
    Guid GenerateGuid(string value);
}

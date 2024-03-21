using System;

namespace NuGettier.Upm;

public interface IGuidFactory
{
    void InitializeWithSeed(string seed);
    Guid GenerateGuid(string value);
}

using System;

namespace NuGettier.Upm;

public interface INpmrcFactory
{
    string GenerateNpmrc(Uri registry, string authToken);
}

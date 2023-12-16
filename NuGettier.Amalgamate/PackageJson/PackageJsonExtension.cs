using System;
using NuGettier.Upm;

namespace NuGettier.Amalgamate;

#nullable enable

public partial record class PackageJson
{
    public override string GenerateReadme(string originalReadme)
    {
        return base.GenerateReadme(originalReadme);
    }
}

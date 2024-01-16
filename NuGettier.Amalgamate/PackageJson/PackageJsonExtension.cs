using System;
using NuGettier.Upm;

namespace NuGettier.Amalgamate;

#nullable enable

public partial record class PackageJson
{
    public override string GenerateReadme(string originalReadme, IReadmeFactory readmeFactory)
    {
        return base.GenerateReadme(originalReadme, readmeFactory);
    }
}

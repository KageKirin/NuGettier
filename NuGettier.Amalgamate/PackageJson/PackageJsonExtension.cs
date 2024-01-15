using System;
using NuGettier.Upm;

namespace NuGettier.Amalgamate;

#nullable enable

public partial record class PackageJson
{
    public override string GenerateReadme(string originalReadme, IReadmeStringFactory readmeStringFactory)
    {
        return base.GenerateReadme(originalReadme, readmeStringFactory);
    }
}

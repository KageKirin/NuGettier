using System;

namespace NuGettier.Amalgamate;

#nullable enable

public partial record class PackageJson : Upm.PackageJson
{
    public PackageJson()
        : base() { }

    public PackageJson(Upm.PackageJson original)
        : base(original) { }
}

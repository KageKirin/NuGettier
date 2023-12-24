using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.CommandLine;
using System.Text.RegularExpressions;
using NuGet.Protocol.Core.Types;
using Microsoft.Extensions.Configuration;
using HandlebarsDotNet;
using NuGettier.Upm;
using Xunit;

namespace NuGettier.Amalgamate;

public partial class Context
{
    protected override string PatchPackageId(string packageId)
    {
        return $"{base.PatchPackageId(packageId)}.amalgamate";
    }

    protected override IDictionary<string, string> PatchPackageDependencies(IDictionary<string, string> dependencies)
    {
        return dependencies
            .Where(d => GetPackageRule(d.Key).IsIgnored == false) //< filter: remove 'ignored' dependencies
            .Where(d => GetPackageRule(d.Key).IsExcluded == true) //< filter: 'excluded' dependencies are not amalgamated
            .ToDictionary(
                //< calling the base method allows to override PatchPackageId, PatchPackageVersion without affecting the dependencies
                d => base.PatchPackageId(d.Key),
                d => base.PatchPackageVersion(d.Key, d.Value)
            );
    }
}

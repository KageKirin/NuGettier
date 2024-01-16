using System;
using System.Collections;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text.RegularExpressions;
using HandlebarsDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NuGet.Protocol.Core.Types;
using NuGettier.Upm;
using Xunit;

namespace NuGettier.Amalgamate;

public partial class Context
{
    public override Upm.PackageJson PatchPackageJson(Upm.PackageJson packageJson)
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());
        var includedDependencies = packageJson
            .Dependencies.Where(d => GetPackageRule(d.Key).IsIgnored == false) //< filter: remove 'ignored' dependencies
            .Where(d => GetPackageRule(d.Key).IsExcluded == false) //< filter: not 'excluded' dependencies are included)
            .ToDictionary(d => d.Key, d => d.Value);

        var patchedPackageJson = base.PatchPackageJson(packageJson);

        patchedPackageJson.DisplayName += " amalgamated with its dependencies";
        patchedPackageJson.Description += "\n\nThis package also contains the following dependency assemblies:";
        patchedPackageJson.Description += string.Join("\n", includedDependencies.Select(d => $"* {d.Key}@{d.Value}"));

        patchedPackageJson.Repository.Directory += ".amalgamate";
        return patchedPackageJson;
    }

    protected override string PatchPackageId(string packageId)
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());
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

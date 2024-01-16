using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using HandlebarsDotNet;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Licenses;
using NuGet.Protocol.Core.Types;
using NuGet.Shared;
using NuGettier.Upm;
using Xunit;

namespace NuGettier.Amalgamate;

public partial class Context
{
    protected override IDictionary<string, string> GetPackageDependencies(
        IPackageSearchMetadata packageSearchMetadata,
        NuGetFramework nugetFramework
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());

        var packageDependencyGroup = NuGetFrameworkUtility.GetNearest<PackageDependencyGroup>(
            packageSearchMetadata.DependencySets,
            nugetFramework
        );

        if (packageDependencyGroup is null)
            return new Dictionary<string, string>();

        return packageDependencyGroup.Packages.ToDictionary(d => d.Id, d => d.VersionRange.ToLegacyShortString());
    }
}

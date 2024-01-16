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
using Xunit;

namespace NuGettier.Upm;

public partial class Context
{
    protected virtual string GetPackageId(IPackageSearchMetadata packageSearchMetadata)
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());
        return packageSearchMetadata.Identity.Id;
    }

    protected virtual string GetPackageVersion(IPackageSearchMetadata packageSearchMetadata)
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());
        return packageSearchMetadata.Identity.Version.ToString();
    }

    protected virtual string? GetPackageLicense(IPackageSearchMetadata packageSearchMetadata)
    {
        return packageSearchMetadata.LicenseMetadata == null
            ? string.Empty
            : packageSearchMetadata.LicenseMetadata.LicenseExpression != null
                ? packageSearchMetadata.LicenseMetadata.LicenseExpression.ToString()
                : !string.IsNullOrEmpty(packageSearchMetadata.LicenseMetadata.License)
                    ? packageSearchMetadata.LicenseMetadata.License
                    : packageSearchMetadata.LicenseMetadata.LicenseUrl.ToString();
    }

    protected virtual string GetPackageDescription(IPackageSearchMetadata packageSearchMetadata)
    {
        return packageSearchMetadata.Description;
    }

    protected virtual string GetPackageDisplayName(IPackageSearchMetadata packageSearchMetadata)
    {
        return packageSearchMetadata.Title;
    }

    protected virtual string? GetPackageHomepage(IPackageSearchMetadata packageSearchMetadata)
    {
        return packageSearchMetadata.ProjectUrl?.ToString();
    }

    protected virtual IEnumerable<string> GetPackageKeywords(IPackageSearchMetadata packageSearchMetadata)
    {
        return packageSearchMetadata.Tags.Split(',', ';', ' ').Where(t => !string.IsNullOrEmpty(t));
    }

    protected virtual Person GetPackageAuthor(IPackageSearchMetadata packageSearchMetadata)
    {
        var firstAuthor = packageSearchMetadata.Authors.Split(',', ';', ' ').First();
        if (string.IsNullOrEmpty(firstAuthor))
        {
            firstAuthor = packageSearchMetadata.Owners.Split(',', ';', ' ').First();
        }

        if (string.IsNullOrEmpty(firstAuthor))
        {
            firstAuthor = @"unknown author, early 21st century";
        }

        return new Person() { Name = firstAuthor, };
    }

    protected virtual IEnumerable<Person> GetPackageContributors(IPackageSearchMetadata packageSearchMetadata)
    {
        var otherAuthors = packageSearchMetadata.Authors.Split(',', ';', ' ');
        if (otherAuthors.Length <= 1)
            return new List<Person>();

        return otherAuthors[1..].Select(author => new Person() { Name = author, });
    }

    protected virtual Repository GetPackageRepository(IPackageSearchMetadata packageSearchMetadata)
    {
        return new Repository()
        {
            RepoType = @"git",
            Url = packageSearchMetadata.ProjectUrl.ToString(),
            Directory = packageSearchMetadata.Identity.Id.ToLowerInvariant(),
        };
    }

    protected virtual PublishingConfiguration GetPackagePublishingConfiguration(
        IPackageSearchMetadata packageSearchMetadata
    )
    {
        return new PublishingConfiguration() { Registry = string.Empty, };
    }

    protected virtual IDictionary<string, string> GetPackageDependencies(
        IPackageSearchMetadata packageSearchMetadata,
        NuGetFramework nugetFramework
    )
    {
        var packageDependencyGroup = NuGetFrameworkUtility.GetNearest<PackageDependencyGroup>(
            packageSearchMetadata.DependencySets,
            nugetFramework
        );

        if (packageDependencyGroup is null)
            return new Dictionary<string, string>();

        return packageDependencyGroup.Packages.ToDictionary(d => d.Id, d => d.VersionRange.ToLegacyShortString());
    }
}

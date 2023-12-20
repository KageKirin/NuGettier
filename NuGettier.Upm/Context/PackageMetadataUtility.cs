using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Licenses;
using NuGet.Protocol.Core.Types;
using NuGet.Shared;
using HandlebarsDotNet;
using Xunit;


namespace NuGettier.Upm;

public partial class Context
{
    protected virtual string GetUpmPackageId(IPackageSearchMetadata packageSearchMetadata)
    {
        return packageSearchMetadata.Identity.Id;
    }

    protected virtual string GetUpmVersion(IPackageSearchMetadata packageSearchMetadata)
    {
        return packageSearchMetadata.Identity.Version.ToString();
    }

    protected virtual string? GetUpmLicense(IPackageSearchMetadata packageSearchMetadata)
    {
        return packageSearchMetadata.LicenseMetadata == null
            ? string.Empty
            : packageSearchMetadata.LicenseMetadata.LicenseExpression != null
                ? packageSearchMetadata.LicenseMetadata.LicenseExpression.ToString()
                : !string.IsNullOrEmpty(packageSearchMetadata.LicenseMetadata.License)
                    ? packageSearchMetadata.LicenseMetadata.License
                    : packageSearchMetadata.LicenseMetadata.LicenseUrl.ToString();
    }

    protected virtual string GetUpmDescription(IPackageSearchMetadata packageSearchMetadata)
    {
        return packageSearchMetadata.Description;
    }

    protected virtual string GetUpmDisplayName(IPackageSearchMetadata packageSearchMetadata)
    {
        return packageSearchMetadata.Title;
    }

    protected virtual string? GetUpmHomepage(IPackageSearchMetadata packageSearchMetadata)
    {
        return packageSearchMetadata.ProjectUrl?.ToString();
    }

    protected virtual IEnumerable<string> GetUpmKeywords(IPackageSearchMetadata packageSearchMetadata)
    {
        return packageSearchMetadata.Tags.Split(',', ';', ' ').Where(t => !string.IsNullOrEmpty(t));
    }

    protected virtual Person GetUpmAuthor(IPackageSearchMetadata packageSearchMetadata)
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

    protected virtual IEnumerable<Person> GetUpmContributors(IPackageSearchMetadata packageSearchMetadata)
    {
        var otherAuthors = packageSearchMetadata.Authors.Split(',', ';', ' ');
        if (otherAuthors.Length <= 1)
            return new List<Person>();

        return otherAuthors[1..].Select(author => new Person() { Name = author, });
    }

    protected virtual Repository GetUpmRepository(IPackageSearchMetadata packageSearchMetadata)
    {
        return new Repository()
        {
            RepoType = @"git",
            Url = packageSearchMetadata.ProjectUrl.ToString(),
            Directory = packageSearchMetadata.Identity.Id.ToLowerInvariant(),
        };
    }

    protected virtual PublishingConfiguration GetUpmPublishingConfiguration(
        IPackageSearchMetadata packageSearchMetadata
    )
    {
        return new PublishingConfiguration() { Registry = string.Empty, };
    }
}

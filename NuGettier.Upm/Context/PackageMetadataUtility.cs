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
}

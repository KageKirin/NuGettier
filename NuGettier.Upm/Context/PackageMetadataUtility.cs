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
}

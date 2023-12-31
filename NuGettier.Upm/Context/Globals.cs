using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using Microsoft.Extensions.Configuration;
using NuGet.Protocol.Core.Types;
using NuGettier;

namespace NuGettier.Upm;

public partial class Context
{
    public static readonly PackageRule DefaultPackageRule =
        new(
            Id: string.Empty,
            Name: @"com.{{{package.author}}}.{{{package.id}}}",
            Version: @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)(\.(?<patch>0|[1-9]\d*))?(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$",
            Framework: string.Empty,
            IsIgnored: false,
            IsExcluded: false,
            IsRecursive: false
        );
}

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
}

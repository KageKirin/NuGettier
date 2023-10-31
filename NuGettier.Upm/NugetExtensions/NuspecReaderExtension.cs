using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using NuGet.Packaging.Core;

namespace NuGettier.Upm;

public static class NuspecReaderExtension
{
    public static string GetUpmPackageName(string author, string id, IEnumerable<Context.PackageRule> packageRules)
    {
        return packageRules.Where(p => p.Id == id && !string.IsNullOrEmpty(p.Name)).Select(p => p.Name).FirstOrDefault()
            ??
            // TODO: use config string + Handlebars template
            $"com.{author}.{id}".ToLowerInvariant().Replace(@" ", @"");
    }
}

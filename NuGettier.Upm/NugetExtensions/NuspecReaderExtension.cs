using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using NuGet.Packaging.Core;

namespace NuGettier.Upm;

public static class NuspecReaderExtension
{
    public static string GetUpmPackageName(
        this NuspecReader nuspecReader,
        IEnumerable<Context.PackageRule> packageRules
    )
    {
        return GetUpmPackageName(nuspecReader.GetAuthors(), nuspecReader.GetId(), packageRules);
    }

    public static string GetUpmPackageName(string author, string id, IEnumerable<Context.PackageRule> packageRules)
    {
        return packageRules.Where(p => p.Id == id && !string.IsNullOrEmpty(p.Name)).Select(p => p.Name).FirstOrDefault()
            ??
            // TODO: use config string + Handlebars template
            $"com.{author}.{id}".ToLowerInvariant().Replace(@" ", @"");
    }

    public static string GetUpmVersion(
        this NuspecReader nuspecReader,
        string? prereleaseSuffix = null,
        string? buildmetaSuffix = null
    )
    {
        var version = nuspecReader.GetVersion().ToString();
        if (prereleaseSuffix != null)
            version += $"-{prereleaseSuffix}";
        if (buildmetaSuffix != null)
            version += $"+{buildmetaSuffix}";

        return version;
    }

    public static string GetUpmName(this NuspecReader nuspecReader)
    {
        return string.IsNullOrWhiteSpace(nuspecReader.GetTitle()) ? nuspecReader.GetId() : nuspecReader.GetTitle();
    }

    public static string GetUpmDisplayName(this NuspecReader nuspecReader, string framework, AssemblyName assemblyName)
    {
        return nuspecReader.GetUpmName()
            + $" ({framework} DLL)"
            + $" [repacked by {assemblyName.Name} v{assemblyName.Version?.ToString()}]";
    }
}

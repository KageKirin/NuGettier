using System;
using System.Text.RegularExpressions;

namespace NuGettier.Core;

public static class StringExtension
{
    public static Regex WildcardToRegex(this string self)
    {
        return new Regex(self.Replace(@".", @"\.").Replace(@"%", @".?").Replace(@"*", @".*"), RegexOptions.Compiled);
    }

    public static void SplitPackageIdVersion(
        this string self,
        out string packageId,
        out string? version,
        out bool latest
    )
    {
        var parts = self.Split(@"@");
        if (parts.Length >= 1)
            packageId = parts[0];
        else
            packageId = self;

        version = null;
        latest = false;

        if (parts.Length >= 2)
        {
            latest = parts[1] == "latest";

            if (!latest)
                version = parts[1];
        }
    }
}

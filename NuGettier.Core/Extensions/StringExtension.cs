using System;
using System.Text.RegularExpressions;

namespace NuGettier.Core;

public static class StringExtension
{
    public static Regex WildcardToRegex(this string self)
    {
        return new Regex(self.Replace(@".", @"\.").Replace(@"%", @".?").Replace(@"*", @".*"), RegexOptions.Compiled);
    }
}

using System;
using System.CommandLine;

namespace NuGettier;

public static partial class Program
{
    private static Argument<string> PackageNameArgument =
        new("packageName", "complete and exact package name");
    private static Argument<string> SearchTermArgument =
        new("searchTerm", "package name-ish or search term");
}

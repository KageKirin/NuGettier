using System;
using System.CommandLine;

namespace NuGettier;

public static partial class Program
{
    private static Argument<string> PackageIdArgument = new("packageId", "complete and exact package id");
    private static Argument<string> SearchTermArgument = new("searchTerm", "package name-ish or search term");
}

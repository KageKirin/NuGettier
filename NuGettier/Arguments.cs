using System;
using System.CommandLine;

namespace NuGettier;

public partial class Program
{
    private Argument<string> PackageIdArgument = new("packageId", "complete and exact package id");
    private Argument<string> PackageIdVersionArgument =
        new("packageIdVersion", "package.id[@version | @latest]");
    private Argument<string> SearchTermArgument = new("searchTerm", "package id-ish or search term");
}

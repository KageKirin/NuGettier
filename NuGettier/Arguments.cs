using System;
using System.CommandLine;

namespace NuGettier;

public partial class NuGettierService
{
    private static Argument<string> PackageIdArgument = new("packageId", "complete and exact package id");
    private static Argument<string> PackageIdVersionArgument =
        new("packageIdVersion", "package.id[@version | @latest]");
    private static Argument<string> SearchTermArgument = new("searchTerm", "package id-ish or search term");
    private static Argument<FileInfo> PackageFileArgument = new("packageFile", "pre-packed UPM package");
    private static Argument<IEnumerable<string>> InputItemsArgument =
        new("inputItems", "input files and folders, and/or wildcard patterns");
}

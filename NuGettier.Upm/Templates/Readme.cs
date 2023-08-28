using System;
using System.Reflection;

namespace NuGettier.Upm.Templates;

public class Readme
{
    public string Name;
    public string Version;
    public string Description;
    public string ReleaseNotes;
    private string ApplicationName;
    private string ApplicationVersion;

    public Readme(string name, string version, string description)
        : this()
    {
        Name = name;
        Version = version;
        Description = description;
    }

    public Readme()
    {
        var executingAssembly = Assembly.GetEntryAssembly();
        var assemblyName = executingAssembly.GetName();
        ApplicationName = assemblyName.Name;
        ApplicationVersion = assemblyName.Version.ToString();
    }

    public string ToString()
    {
        return @$"# {Name} - {Version}

** Repacked from NuGet using {ApplicationName} v{ApplicationVersion} **

{Description}

{(string.IsNullOrWhiteSpace(ReleaseNotes) ? "" : "## Release Notes\n")}
{ReleaseNotes}
";
    }
}

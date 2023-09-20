using System;
using System.Reflection;

namespace NuGettier.Upm.Templates;

public class Readme
{
    public string Name = string.Empty;
    public string Version = string.Empty;
    public string Description = string.Empty;
    public string ReleaseNotes = string.Empty;
    public string ApplicationName = string.Empty;
    public string ApplicationVersion = string.Empty;

    public Readme() { }

    public Readme(
        string name,
        string version,
        string description,
        string applicationName,
        string applicationVersion
    )
    {
        Name = name;
        Version = version;
        Description = description;
        ApplicationName = applicationName;
        ApplicationVersion = applicationVersion;
    }

    public Readme(string name, string version, string description, AssemblyName assemblyName)
        : this(name, version, description, assemblyName.Name, assemblyName.Version.ToString()) { }

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

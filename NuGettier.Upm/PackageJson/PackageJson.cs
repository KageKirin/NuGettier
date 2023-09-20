using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NuGettier.Upm;

public class PackageJson
{
    public sealed class StringStringDictionary : Dictionary<string, string> { }

    public string Name = String.Empty;
    public string name
    {
        get { return Name; }
        set { Name = value; }
    }

    public string Version = String.Empty;
    public string version
    {
        get { return Version; }
        set { Version = value; }
    }

    public string? License = null;
    public string? license
    {
        get { return License; }
        set { License = value; }
    }

    public string DisplayName = String.Empty;
    public string displayName
    {
        get { return DisplayName; }
        set { DisplayName = value; }
    }

    public string Description = String.Empty;
    public string description
    {
        get { return Description; }
        set { Description = value; }
    }

    public Author Author = new Author();
    public Author author
    {
        get { return Author; }
        set { Author = value; }
    }

    public List<string> Files = new List<string>();
    public List<string> files
    {
        get { return Files; }
        set { Files = value; }
    }

    public StringStringDictionary Dependencies = new StringStringDictionary();
    public StringStringDictionary dependencies
    {
        get { return Dependencies; }
        set { Dependencies = value; }
    }

    public List<string> Keywords = new List<string>();
    public List<string> keywords
    {
        get { return Keywords; }
        set { Keywords = value; }
    }

    public Repository Repository = new Repository();
    public Repository repository
    {
        get { return Repository; }
        set { Repository = value; }
    }

    public PublishingConfiguration PublishingConfiguration = new PublishingConfiguration();
    public PublishingConfiguration publishingConfiguration
    {
        get { return PublishingConfiguration; }
        set { PublishingConfiguration = value; }
    }

    public static PackageJson? FromJson(in string json)
    {
        return JsonSerializer.Deserialize<PackageJson>(json);
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}

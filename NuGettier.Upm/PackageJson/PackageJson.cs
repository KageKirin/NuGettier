using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NuGettier.Upm;

public partial record class PackageJson
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("license")]
    public string License { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("unity")]
    public string MinUnityVersion { get; set; } = "2023.1";

    [JsonPropertyName("dotnetframework")]
    public string DotNetFramework { get; set; } = "netstandard2.1";

    [JsonPropertyName("homepage")]
    public string? Homepage { get; set; }

    [JsonPropertyName("author")]
    public Person Author { get; set; } = new Person();

    [JsonPropertyName("contributors")]
    public IEnumerable<Person> Contributors { get; set; } = new List<Person>();

    [JsonPropertyName("files")]
    public IList<string> Files { get; set; } = new List<string>() { @"**.meta", @"**.dll", @"**.xml", @"**.md", };

    [JsonPropertyName("dependencies")]
    public IDictionary<string, string> Dependencies { get; set; } = new StringStringDictionary();

    [JsonPropertyName("devDependencies")]
    public IDictionary<string, string>? DevDependencies { get; set; }

    [JsonPropertyName("keywords")]
    public IEnumerable<string> Keywords { get; set; } = new List<string>();

    [JsonPropertyName("repository")]
    public Repository Repository { get; set; } = new Repository();

    [JsonPropertyName("publishConfig")]
    public PublishingConfiguration PublishingConfiguration { get; set; } = new PublishingConfiguration();

    public static PackageJson? FromJson(in string json)
    {
        return JsonSerializer.Deserialize<PackageJson>(json);
    }

    public virtual string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}

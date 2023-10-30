using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NuGettier.Upm;

public record class PackageJson
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = String.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = String.Empty;

    [JsonPropertyName("license")]
    public string? License { get; set; } = null;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = String.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = String.Empty;

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

    [JsonPropertyName("publishingConfiguration")]
    public PublishingConfiguration PublishingConfiguration { get; set; } = new PublishingConfiguration();

    public static PackageJson? FromJson(in string json)
    {
        return JsonSerializer.Deserialize<PackageJson>(json);
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}

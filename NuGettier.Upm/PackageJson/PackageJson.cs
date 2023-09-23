using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NuGettier.Upm;

public class PackageJson
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

    [JsonPropertyName("author")]
    public Author Author { get; set; } = new Author();

    [JsonPropertyName("files")]
    public List<string> Files { get; set; } = new List<string>() { @"**.meta", @"**.dll", @"**.xml", @"**.md", };

    [JsonPropertyName("dependencies")]
    public StringStringDictionary Dependencies { get; set; } = new StringStringDictionary();

    [JsonPropertyName("keywords")]
    public List<string> Keywords { get; set; } = new List<string>();

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

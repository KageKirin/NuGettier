using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NuGettier.Upm;

public record class Repository
{
    [JsonPropertyName("type")]
    public string RepoType { get; set; } = String.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = String.Empty;

    [JsonPropertyName("directory")]
    public string Directory { get; set; } = String.Empty;
}

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NuGettier.Upm;

public record class Author
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = String.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = String.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = String.Empty;
}

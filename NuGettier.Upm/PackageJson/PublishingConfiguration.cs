using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NuGettier.Upm;

public class PublishingConfiguration
{
    [JsonPropertyName("registry")]
    public string Registry { get; set; } = String.Empty;
}

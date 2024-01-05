using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NuGettier;

public partial class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, };
}

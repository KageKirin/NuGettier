using System;

namespace NuGettier.Upm;

[Flags]
public enum GuidFormat
{
    None = 0,
    Unity = 1,
    Rfc4122 = 2,
    Unity_Rfc4122 = Unity | Rfc4122,
    Mask = Unity | Rfc4122,
}

public sealed class GuidFactorySettings
{
    public string Algorithm { get; set; } = string.Empty;
    public GuidFormat Format { get; set; } = GuidFormat.None;
}

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NuGettier.Upm;

public class PublishingConfiguration
{
    public string Registry = String.Empty;
    public string registry
    {
        get { return Registry; }
        set { Registry = value; }
    }
}

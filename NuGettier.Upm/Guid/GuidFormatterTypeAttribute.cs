using System;
using System.ComponentModel.DataAnnotations;

namespace NuGettier.Upm;

[AttributeUsage(AttributeTargets.Class)]
public class GuidFormatterTypeAttribute(GuidFormat format) : Attribute()
{
    public GuidFormat Format { get; set; } = format;
}

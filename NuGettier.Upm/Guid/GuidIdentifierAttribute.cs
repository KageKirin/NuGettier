using System;
using System.ComponentModel.DataAnnotations;

namespace NuGettier.Upm;

[AttributeUsage(AttributeTargets.Class)]
public class GuidIdentifierAttribute([Required(AllowEmptyStrings = false)] string identifier) : Attribute()
{
    [Required(AllowEmptyStrings = false)]
    public string Identifier { get; set; } = identifier;
}

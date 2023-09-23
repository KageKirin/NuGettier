using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NuGettier.Upm;

public sealed class StringStringDictionary : Dictionary<string, string>
{
    public StringStringDictionary()
        : base() { }

    public StringStringDictionary(StringStringDictionary reference)
        : this(reference as Dictionary<string, string>) { }

    public StringStringDictionary(Dictionary<string, string> reference)
        : base(reference) { }

    public StringStringDictionary(IEnumerable<KeyValuePair<string, string>> reference)
        : base(reference) { }
}

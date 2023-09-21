using System;
using System.Text;
using System.Collections.Generic;

namespace NuGettier.Upm.TarGz;

public class FileDictionary : Dictionary<string, byte[]>, IDisposable
{
    public FileDictionary()
        : base() { }

    public FileDictionary(FileDictionary reference)
        : this(reference as Dictionary<string, byte[]>) { }

    public FileDictionary(Dictionary<string, byte[]> reference)
        : base(reference) { }

    public FileDictionary(IEnumerable<KeyValuePair<string, byte[]>> reference)
        : base(reference) { }

    public virtual void Dispose()
    {
        Clear();
    }

    public virtual new byte[] this[string key]
    {
        get => base[key];
        set => base[key] = value;
    }

    public virtual new void Add(string key, byte[] value)
    {
        base.Add(key, value);
    }

    public virtual void Add(KeyValuePair<string, byte[]> keyValuePair)
    {
        this.Add(keyValuePair.Key, keyValuePair.Value);
    }

    public virtual void Add(string key, string value)
    {
        this.Add(key, Encoding.Default.GetBytes(value));
    }

    public virtual void Add(KeyValuePair<string, string> keyValuePair)
    {
        this.Add(keyValuePair.Key, keyValuePair.Value);
    }

    public virtual void AddRange(FileDictionary files)
    {
        foreach (var kvp in files)
        {
            this.Add(kvp);
        }
    }

    public virtual void AddRange(IEnumerable<KeyValuePair<string, byte[]>> keyValuePairs)
    {
        foreach (var kvp in keyValuePairs)
        {
            this.Add(kvp);
        }
    }

    public virtual void AddRange(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
    {
        foreach (var kvp in keyValuePairs)
        {
            this.Add(kvp);
        }
    }
}

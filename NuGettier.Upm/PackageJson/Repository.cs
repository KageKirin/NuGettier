using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NuGettier.Upm;

public class Repository
{
    public string RepoType = String.Empty;
    public string type
    {
        get { return RepoType; }
        set { RepoType = value; }
    }

    public string Url = String.Empty;
    public string url
    {
        get { return Url; }
        set { Url = value; }
    }

    public string Directory = String.Empty;
    public string directory
    {
        get { return Directory; }
        set { Directory = value; }
    }
}

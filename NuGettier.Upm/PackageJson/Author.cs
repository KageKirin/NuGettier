using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NuGettier.Upm;

public class Author
{
    public string Name = String.Empty;
    public string name
    {
        get { return Name; }
        set { Name = value; }
    }

    public string Email = String.Empty;
    public string email
    {
        get { return Email; }
        set { Email = value; }
    }

    public string Url = String.Empty;
    public string url
    {
        get { return Url; }
        set { Url = value; }
    }
}

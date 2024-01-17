using System;

namespace NuGettier.Upm;

public static class UriExtension
{
    /// <summary>
    /// allows to get the schemeless Uri as required by `npm`
    /// https://my-awesome-server/npmapi -> my-awesome-server/npmapi
    /// </summary>
    /// <returns>Uri.AbsoluteUri without the scheme</returns>
    public static string SchemelessUri(this Uri uri)
    {
        return uri.AbsoluteUri.Replace($"{uri.Scheme}//", "").Replace("https://", "").Replace("http://", "");
    }
}

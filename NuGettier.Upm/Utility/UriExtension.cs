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

    /// <summary>
    /// allows to get the scopeless Uri as required by `npm`
    /// https://my-awesome-server/npmapi/@scope -> my-awesome-server/npmapi/
    /// </summary>
    /// <returns>Uri.AbsoluteUri without the scope</returns>
    public static string ScopelessAbsoluteUri(this Uri uri)
    {
        var scope = uri.Scope();
        if (string.IsNullOrEmpty(scope))
            return uri.AbsoluteUri;

        return uri.AbsoluteUri.Replace(scope, "");
    }

    /// <summary>
    /// allows to get the schemeless Uri as required by `npm`
    /// https://my-awesome-server/npmapi/@scope -> @scope
    /// </summary>
    /// <returns>the scope</returns>
    public static string? Scope(this Uri uri)
    {
        var scopeIndex = uri.AbsolutePath.LastIndexOf(@"@");
        if (scopeIndex < 0)
            return null;

        return uri.AbsolutePath.Substring(scopeIndex);
    }
}

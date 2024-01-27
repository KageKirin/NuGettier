using System;

namespace NuGettier.Upm;

public static class UriExtension
{
    /// <summary>
    /// returns the Uri in the format required by `npm` with a single trailing '/'
    /// https://my-awesome-server/npmapi/@scope -> https://my-awesome-server/npmapi/@scope/
    /// https://my-awesome-server/npmapi/@scope/ -> https://my-awesome-server/npmapi/@scope/
    /// https://my-awesome-server/npmapi -> https://my-awesome-server/npmapi/
    /// https://my-awesome-server/npmapi/ -> https://my-awesome-server/npmapi/
    /// </summary>
    /// <returns>Uri as string with a single trailing slash</returns>
    public static Uri ToNpmFormat(this Uri uri)
    {
        return new Uri(uri.AbsoluteUri.ToNpmFormat());
    }

    public static string ToNpmFormat(this string uri)
    {
        return $"{uri.TrimEnd('/')}/";
    }

    /// <summary>
    /// returns the schemeless Uri as required by `npm`
    /// https://my-awesome-server/npmapi/@scope -> my-awesome-server/npmapi
    /// </summary>
    /// <returns>Uri.AbsoluteUri without the scheme nor the scope</returns>
    public static string SchemelessUri(this Uri uri)
    {
        return uri.ScopelessAbsoluteUri().Replace($"{uri.Scheme}://", "").ToNpmFormat();
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
            return uri.AbsoluteUri.ToNpmFormat();

        return uri.AbsoluteUri.Replace(scope, "").ToNpmFormat();
    }

    /// <summary>
    /// allows to get the schemeless Uri as required by `npm`
    /// https://my-awesome-server/npmapi/@scope -> @scope
    /// </summary>
    /// <returns>the scope</returns>
    public static string Scope(this Uri uri)
    {
        var scopeIndex = uri.AbsolutePath.LastIndexOf(@"@");
        if (scopeIndex < 0)
            return string.Empty;

        return uri.AbsolutePath.Substring(scopeIndex);
    }
}

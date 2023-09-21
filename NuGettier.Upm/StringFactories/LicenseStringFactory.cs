using System;
using System.Net;
using System.Reflection;
using System.Threading;
using HandlebarsDotNet;

namespace NuGettier.Upm;

public static class LicenseStringFactory
{
    private static async Task<string> GetLicense(Uri licenseUrl)
    {
        HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(licenseUrl);
        httpRequest.Timeout = 10000; // 10 secs
        httpRequest.UserAgent = "NuGettier";

        HttpWebResponse webResponse = (HttpWebResponse)await httpRequest.GetResponseAsync();
        StreamReader responseStream = new StreamReader(webResponse.GetResponseStream());

        return await responseStream.ReadToEndAsync();
    }

    public static string GenerateLicense(
        string name,
        string version,
        string copyright,
        string copyrightHolder,
        string license,
        string licenseUrl
    )
    {
        if (string.IsNullOrEmpty(copyright))
        {
            copyright = $"(C) copyrightHolder, published under {license}.";
        }

        var licenseText = string.IsNullOrEmpty(licenseUrl)
            ? license
            : Task.Run<string>(async () => await GetLicense(new Uri(licenseUrl))).Result;

        var template = Handlebars.Compile(
            EmbeddedAssetHelper.GetEmbeddedResourceString("NuGettier.Upm.Templates.LICENSE.md")
        );
        return template(
            new
            {
                Name = name,
                Version = version,
                Copyright = copyright,
                License = licenseText,
            }
        );
    }
}

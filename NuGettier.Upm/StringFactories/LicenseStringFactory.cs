using System;
using System.Net;
using System.Reflection;
using System.Threading;
using HandlebarsDotNet;

namespace NuGettier.Upm;

public interface ILicenseStringFactory
{
    string GenerateLicense(
        string name,
        string version,
        string copyright,
        string copyrightHolder,
        string license,
        string licenseUrl
    );
}

public static class LicenseStringFactory
{
    private static async Task<string> GetLicense(Uri licenseUrl)
    {
        HttpClientHandler httpClientHandler = new() { UseDefaultCredentials = true, };
        HttpClient httpClient = new(httpClientHandler) { Timeout = TimeSpan.FromSeconds(10), };
        var response = await httpClient.GetAsync(licenseUrl);
        StreamReader responseStream = new(response.Content.ReadAsStream());

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

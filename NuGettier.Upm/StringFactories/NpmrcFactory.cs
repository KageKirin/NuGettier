using System;
using System.Text;

namespace NuGettier.Upm;

#nullable enable

public interface INpmrcFactory
{
    string GenerateNpmrc(Uri registry, string? authToken);
}

public class NpmrcFactory : INpmrcFactory, IDisposable
{
    public virtual string GenerateNpmrc(Uri registry, string? authToken)
    {
        StringBuilder builder = new();

        var uriScope = registry.Scope();
        // `//registry=${registry}/`
        builder.AppendLine($"//registry={registry.ScopelessAbsoluteUri()}");

        if (!string.IsNullOrEmpty(uriScope))
        {
            // `//${uriScope}:registry=${registry}/`
            builder.AppendLine($"//{uriScope}:registry={registry.ScopelessAbsoluteUri()}");
        }

        if (!string.IsNullOrEmpty(authToken))
        {
            // `//${host}/:_authToken=${authToken}`
            builder.AppendLine($"//{registry.Host}/:_authToken={authToken}");

            if (registry.Host != registry.Authority)
            {
                // `//${host}:${port}/:_authToken=${authToken}`
                builder.AppendLine($"//{registry.Authority}/:_authToken={authToken}");
            }
        }

        return builder.ToString();
    }

    public virtual void Dispose() { }
}

using Microsoft.Extensions.Configuration;

public class AppSettings
{
    public string AppName { get; set; }
    public string AppVersion { get; set; }

    public AppSettings(IConfiguration configuration)
    {
        AppName = configuration["AppName"] ?? @"default_AppName";
        AppVersion = configuration["AppVersion"] ?? @"default_AppVersion";
    }
}

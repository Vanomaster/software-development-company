using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Dal;

/// <summary>
/// Configuration helper.
/// </summary>
public class ConfigurationHelper : IConfigurationHelper
{
    private static readonly IConfigurationRoot ConfigurationRoot = GetConfigurationRoot();

    /// <inheritdoc/>
    public string? MainConnectionString => ConfigurationRoot.GetConnectionString("Main");

    private static IConfigurationRoot GetConfigurationRoot()
    {
        string currentDirectoryPath = Directory.GetCurrentDirectory();
        var configDirectoryPath = $@"{currentDirectoryPath}\Configuration";
        var configDirectoryInfo = new DirectoryInfo(configDirectoryPath);
        var configFiles = configDirectoryInfo.EnumerateFiles("config*.json").ToList();
        if (!configFiles.Any())
        {
            /*var sourceFileName = $@"{currentDirectoryPath}..\..\..\..\..\Dal\Configuration\config.json";
            var destFileName = $@"{currentDirectoryPath}\config.json";
            File.Copy(sourceFileName, destFileName);
            configFiles = directoryInfo.EnumerateFiles("config*.json").ToList();*/
            throw new FileNotFoundException($"config.json file was not found in {currentDirectoryPath}.");
        }

        var configFile = configFiles.First();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(configFile.FullName, optional: false, reloadOnChange: true)
            .Build();

        return configuration;
    }
}
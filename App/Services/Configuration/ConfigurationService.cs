using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia.Platform;
using Microsoft.Extensions.Configuration;

namespace App.Services.Configuration;

public static class ConfigurationService
{
  private static readonly Lazy<AppSettings> CachedAppSettings = new(GetAppSettings);

  private static readonly Lazy<AppMetadata> CachedMetadata = new(GetMetadata);

  private static readonly Lazy<AppFolders> CachedFolders = new(GetAppFolders);

  public static AppFolders AppFolders => CachedFolders.Value;

  public static AppSettings AppSettings => CachedAppSettings.Value;

  public static AppMetadata AppMetadata => CachedMetadata.Value;

  private static AppSettings GetAppSettings()
  {
    using var stream = AssetLoader.Open(new System.Uri($"avares://{AppMetadata.AssemblyName}/Assets/Config/appsettings.json"));
    var configuration = new ConfigurationBuilder().AddJsonStream(stream).Build();
    var settings = configuration.Get<AppSettings>() ?? throw new InvalidOperationException("Configuration is missing or invalid.");

    ValidateSettings(settings);
    return settings;
  }

  private static AppMetadata GetMetadata()
  {
    var assembly = Assembly.GetExecutingAssembly();

    return new AppMetadata
    {
      AppName = GetAttribute<AssemblyTitleAttribute>(assembly, attribute => attribute.Title),
      AppVersion = GetAttribute<AssemblyInformationalVersionAttribute>(assembly, attribute => attribute.InformationalVersion),
      CompanyName = GetAttribute<AssemblyCompanyAttribute>(assembly, attribute => attribute.Company),
      Description = GetAttribute<AssemblyDescriptionAttribute>(assembly, attribute => attribute.Description),
      Copyright = GetAttribute<AssemblyCopyrightAttribute>(assembly, attribute => attribute.Copyright),
      AssemblyName = assembly.GetName().Name ?? throw new InvalidOperationException("Assembly name is missing")
    };
  }

  private static AppFolders GetAppFolders()
  {
    return new AppFolders
    {
      BinFolder = GetAppBinFolder(),
      DataFolder = GetAppDataFolder(),
      LogFolder = GetAppLogFolder()
    };
  }

  private static string GetAppBinFolder()
  {
    return Path.Combine(AppContext.BaseDirectory, "bin");
  }

  private static string GetAttribute<T>(Assembly assembly, Func<T, string?> selector) where T : Attribute
  {
    var attribute = assembly.GetCustomAttribute<T>() ?? throw new InvalidOperationException($"Missing {typeof(T).Name}");

    var value = selector(attribute);

    return !string.IsNullOrWhiteSpace(value) ? value : throw new InvalidOperationException($"Empty {typeof(T).Name}");
  }

  private static void ValidateSettings(AppSettings settings)
  {
    var invalid = typeof(AppSettings).GetProperties()
      .Where(p => p.PropertyType == typeof(string) && string.IsNullOrWhiteSpace((string?)p.GetValue(settings)))
      .Select(p => p.Name)
      .ToList();

    if (invalid.Count > 0) throw new InvalidOperationException($"Invalid settings: {string.Join(", ", invalid)}");
  }

  private static string GetAppDataFolder()
  {
    var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppMetadata.CompanyName, AppMetadata.AppName);

    Directory.CreateDirectory(folderPath);

    return folderPath;
  }

  private static string GetAppLogFolder()
  {
    var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppMetadata.CompanyName, AppMetadata.AppName,
      AppSettings.LogFolder);

    Directory.CreateDirectory(folderPath);

    return folderPath;
  }
}

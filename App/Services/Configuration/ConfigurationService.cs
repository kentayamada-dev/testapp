using System;
using System.Linq;
using System.Reflection;
using Avalonia.Platform;
using Microsoft.Extensions.Configuration;

namespace App.Services.Configuration;

public static class ConfigurationService
{
  private static readonly Lazy<AppSettings> CachedAppSettings =
    new(GetAppSettingsInternal);

  private static readonly Lazy<AppMetadata> CachedMetadata =
    new(GetMetadataInternal);

  public static AppSettings AppSettings => CachedAppSettings.Value;

  public static AppMetadata AppMetadata => CachedMetadata.Value;

  private static AppSettings GetAppSettingsInternal()
  {
    using var stream = AssetLoader.Open(
      new Uri("avares://App/Assets/Config/appsettings.json"));
    var configuration = new ConfigurationBuilder()
      .AddJsonStream(stream)
      .Build();
    var settings = configuration.Get<AppSettings>()
                   ?? throw new InvalidOperationException(
                     "Configuration is missing or invalid.");

    ValidateSettings(settings);
    return settings;
  }

  private static AppMetadata GetMetadataInternal()
  {
    var assembly = Assembly.GetExecutingAssembly();

    return new AppMetadata
    {
      AppName = GetAttribute<AssemblyTitleAttribute>(
        assembly,
        attribute => attribute.Title),
      AppVersion = GetAttribute<AssemblyInformationalVersionAttribute>(
        assembly,
        attribute => attribute.InformationalVersion),
      CompanyName = GetAttribute<AssemblyCompanyAttribute>(
        assembly,
        attribute => attribute.Company)
    };
  }

  private static string GetAttribute<T>(
    Assembly assembly,
    Func<T, string?> selector) where T : Attribute
  {
    var attribute = assembly.GetCustomAttribute<T>()
                    ?? throw new InvalidOperationException(
                      $"Missing {typeof(T).Name}");

    var value = selector(attribute);
    return !string.IsNullOrWhiteSpace(value)
      ? value
      : throw new InvalidOperationException(
        $"Empty {typeof(T).Name}");
  }

  private static void ValidateSettings(AppSettings settings)
  {
    var invalid = typeof(AppSettings)
      .GetProperties()
      .Where(p => p.PropertyType == typeof(string) &&
                  string.IsNullOrWhiteSpace((string?)p.GetValue(settings)))
      .Select(p => p.Name)
      .ToList();

    if (invalid.Count > 0)
      throw new InvalidOperationException(
        $"Invalid settings: {string.Join(", ", invalid)}");
  }
}

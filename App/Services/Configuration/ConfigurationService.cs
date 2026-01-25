using System;
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
      AppName = assembly
        .GetCustomAttribute<AssemblyTitleAttribute>()?
        .Title ?? throw new InvalidOperationException(
        "AssemblyTitleAttribute not found."),
      AppVersion = assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion ?? throw new InvalidOperationException(
        "AssemblyInformationalVersionAttribute not found."),
      CompanyName = assembly
        .GetCustomAttribute<AssemblyCompanyAttribute>()?
        .Company ?? throw new InvalidOperationException(
        "AssemblyCompanyAttribute not found.")
    };
  }

  private static void ValidateSettings(AppSettings settings)
  {
    if (string.IsNullOrWhiteSpace(settings.LogFolder))
      throw new InvalidOperationException(
        "LogFolder cannot be empty.");

    if (string.IsNullOrWhiteSpace(settings.AppcastUrl))
      throw new InvalidOperationException(
        "AppcastUrl cannot be empty.");

    if (string.IsNullOrWhiteSpace(settings.SettingsFile))
      throw new InvalidOperationException(
        "SettingsFile cannot be empty.");

    if (string.IsNullOrWhiteSpace(settings.PublicKey))
      throw new InvalidOperationException(
        "PublicKey cannot be empty.");
  }
}

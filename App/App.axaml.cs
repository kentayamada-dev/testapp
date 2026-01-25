using System;
using System.IO;
using App.Services;
using App.Services.Configuration;
using App.Services.Culture;
using App.Services.Data;
using App.Services.Theme;
using App.Services.Updater;
using App.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace App;

public sealed class App : Application
{
  private CultureService? _cultureService;
  private MainWindow? _mainWindow;
  private ThemeService? _themeService;
  private UpdaterService? _updaterService;
  public static string? AppName { get; private set; }
  public static IServiceProvider? ServiceProvider { get; private set; }

  public override void Initialize()
  {
    AppName = ConfigurationService.AppMetadata.AppName;

    AvaloniaXamlLoader.Load(this);
  }

  public override void OnFrameworkInitializationCompleted()
  {
    var appDataFolder = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
      ConfigurationService.AppMetadata.CompanyName,
      ConfigurationService.AppMetadata.AppName);

    var services = new ServiceCollection();

    services.AddAppServices(appDataFolder);

    ServiceProvider = services.BuildServiceProvider();

    _cultureService = ServiceProvider.GetRequiredService<CultureService>();

    _themeService = ServiceProvider.GetRequiredService<ThemeService>();

    _updaterService = ServiceProvider.GetRequiredService<UpdaterService>();

    var dataPersistService =
      ServiceProvider.GetRequiredService<DataPersistService>();

    _cultureService.SetCulture(
      CultureService.GetCulture(dataPersistService.Get(DataKey.Culture)),
      false);

    _themeService.SetTheme(
      ThemeService.GetTheme(dataPersistService.Get(DataKey.Theme)),
      false);

    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      _mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
      desktop.MainWindow = _mainWindow;
    }

    base.OnFrameworkInitializationCompleted();
  }

  private void ChangeCultureJp_Click(object? sender, EventArgs e)
  {
    _cultureService?.SetCulture(Culture.Jp);
  }

  private void ChangeCultureEn_Click(object? sender, EventArgs e)
  {
    _cultureService?.SetCulture(Culture.En);
  }

  private async void UpdateCheck_Click(object? sender, EventArgs e)
  {
    if (_mainWindow == null || _updaterService == null)
      throw new InvalidOperationException(
        "Required services are not initialized");

    await _updaterService.CheckForUpdate(_mainWindow);
  }

  private void ChangeThemeDark_Click(object? sender, EventArgs e)
  {
    _themeService?.SetTheme(Theme.Dark);
  }

  private void ChangeThemeLight_Light(object? sender, EventArgs e)
  {
    _themeService?.SetTheme(Theme.Light);
  }

  private void ChangeThemeSystem_Click(object? sender, EventArgs e)
  {
    _themeService?.SetTheme(Theme.System);
  }
}

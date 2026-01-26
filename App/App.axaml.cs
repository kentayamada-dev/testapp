using System;
using System.IO;
using App.Services;
using App.Services.Configuration;
using App.Services.Culture;
using App.Services.Data;
using App.Services.Theme;
using App.Services.Updater;
using App.Services.Uri;
using App.ViewModels;
using App.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace App;

public sealed class App : Application
{
  private CultureService? _cultureService;
  private Window? _mainWindow;
  private ThemeService? _themeService;
  private UpdaterService? _updaterService;
  public static string? AppName { get; private set; }

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

    var serviceProvider = services.BuildServiceProvider();

    _cultureService = serviceProvider.GetRequiredService<CultureService>();

    _themeService = serviceProvider.GetRequiredService<ThemeService>();

    _updaterService = serviceProvider.GetRequiredService<UpdaterService>();

    var dataPersistService =
      serviceProvider.GetRequiredService<DataPersistService>();

    _cultureService.SetCulture(CultureService.GetCulture(dataPersistService.Get(DataKey.Culture)), false);

    _themeService.SetTheme(ThemeService.GetTheme(dataPersistService.Get(DataKey.Theme)), false);

    var mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      _mainWindow = new MainWindow { DataContext = mainViewModel };

      mainViewModel.SetMainWindow(_mainWindow);

      desktop.MainWindow = _mainWindow;
    }

    base.OnFrameworkInitializationCompleted();
  }

  private void CheckUpdate_Click(object? sender, EventArgs e)
  {
    if (_mainWindow == null) return;

    _updaterService?.CheckForUpdate(_mainWindow);
  }

  private void AboutDeveloper_Click(object? sender, EventArgs e)
  {
    if (_mainWindow == null) return;

    _ = UriService.OpenUri(ConfigurationService.AppSettings.HomepageUrl, _mainWindow);
  }

  private void AboutApp_Click(object? sender, EventArgs e)
  {
    if (_mainWindow == null) return;

    _ = UriService.OpenUri(ConfigurationService.AppSettings.AppRepoUrl, _mainWindow);
  }
}

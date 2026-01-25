using System;
using System.IO;
using App.Services;
using App.Services.Configuration;
using App.Services.Culture;
using App.Services.Data;
using App.Services.Theme;
using App.ViewModels;
using App.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace App;

public sealed class App : Application
{
  private CultureService? _cultureService;
  private ThemeService? _themeService;
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

    var dataPersistService =
      serviceProvider.GetRequiredService<DataPersistService>();

    _cultureService.SetCulture(
      CultureService.GetCulture(dataPersistService.Get(DataKey.Culture)),
      false);

    _themeService.SetTheme(
      ThemeService.GetTheme(dataPersistService.Get(DataKey.Theme)),
      false);

    var mainCiewModel = serviceProvider.GetRequiredService<MainViewModel>();
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      var mainWindow = new MainWindow
      {
        DataContext = mainCiewModel
      };

      mainCiewModel.SetMainWindow(mainWindow);

      desktop.MainWindow = mainWindow;
    }

    base.OnFrameworkInitializationCompleted();
  }
}

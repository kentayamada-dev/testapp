using System;
using System.Globalization;
using App.Services;
using App.Services.Application;
using App.Services.Configuration;
using App.Services.Culture;
using App.Services.Data;
using App.Services.Logger;
using App.Services.Theme;
using App.Services.Updater;
using App.Services.Uri;
using App.ViewModels;
using App.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.DependencyInjection;

namespace App;

public sealed class App : Application
{
  private Window? _mainWindow;
  private UpdaterService? _updaterService;
  public static string? AppName { get; private set; }

  public override void Initialize()
  {
    AppName = ConfigurationService.AppMetadata.AppName;

    DataPersistService.Initialize();

    var savedCulture = CultureService.GetCulture(DataPersistService.Get().Culture);

    if (CultureInfo.CurrentCulture.Name != savedCulture.Code) CultureService.ApplyCulture(savedCulture);

    AvaloniaXamlLoader.Load(this);
  }

  public override void OnFrameworkInitializationCompleted()
  {
    var services = new ServiceCollection();

    services.AddAppServices();

    var serviceProvider = services.BuildServiceProvider();

    _updaterService = serviceProvider.GetRequiredService<UpdaterService>();

    ThemeService.ApplyTheme(ThemeService.GetTheme(DataPersistService.Get().Theme));

    var mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      var shutdownMode = OperatingSystem.IsMacOS()
        ? ShutdownMode.OnExplicitShutdown
        : ShutdownMode.OnLastWindowClose;

      _mainWindow = new MainWindow { DataContext = mainViewModel };
      mainViewModel.SetMainWindow(_mainWindow);

      desktop.ShutdownMode = shutdownMode;
      desktop.MainWindow = _mainWindow;
    }

    if (OperatingSystem.IsWindows()) SetTrayIcon();

    base.OnFrameworkInitializationCompleted();
  }

  private void SetTrayIcon()
  {
    var quitMenuItem = new NativeMenuItem(
      Assets.Culture.Resources.QuitApp
    );
    quitMenuItem.Click += QuitApp_Click;

    var trayIcon = new TrayIcon
    {
      Icon = new WindowIcon(
        new Bitmap(
          AssetLoader.Open(
            new Uri("avares://App/Assets/Logo/logo.ico")
          )
        )
      ),
      ToolTipText = AppName,
      Menu = [quitMenuItem]
    };

    trayIcon.Clicked += TrayIcon_Click;

    var icons = new TrayIcons { trayIcon };
    TrayIcon.SetIcons(Current ?? throw new InvalidOperationException("Application context unavailable."), icons);
  }

  private async void CheckUpdate_Click(object? sender, EventArgs e)
  {
    try
    {
      if (_mainWindow == null || _updaterService == null) return;

      await _updaterService.CheckForUpdate(_mainWindow);
    }
    catch (Exception ex)
    {
      await LoggerService.Log(ex);
    }
  }

  private async void AboutDeveloper_Click(object? sender, EventArgs e)
  {
    try
    {
      if (_mainWindow == null) return;

      await UriService.OpenUri(ConfigurationService.AppSettings.HomepageUrl, _mainWindow);
    }
    catch (Exception ex)
    {
      await LoggerService.Log(ex);
    }
  }

  private async void AboutApp_Click(object? sender, EventArgs e)
  {
    try
    {
      if (_mainWindow == null) return;

      await UriService.OpenUri(ConfigurationService.AppSettings.AppRepoUrl, _mainWindow);
    }
    catch (Exception ex)
    {
      await LoggerService.Log(ex);
    }
  }

  private static void QuitApp_Click(object? sender, EventArgs e)
  {
    ApplicationService.CloseApplication();
  }

  private void TrayIcon_Click(object? sender, EventArgs e)
  {
    _mainWindow?.Show();
    _mainWindow?.Activate();
  }
}

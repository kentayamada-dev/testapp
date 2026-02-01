using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using App.Services;
using App.Services.Application;
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
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.DependencyInjection;

namespace App;

public sealed class App : Application
{
  private Window? _mainWindow;
  private SingleInstanceService? _singleInstanceService;
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
    _singleInstanceService = Program.SingleInstanceService;

    var services = new ServiceCollection();
    services.AddAppServices();
    var serviceProvider = services.BuildServiceProvider();

    _updaterService = serviceProvider.GetRequiredService<UpdaterService>();

    ThemeService.ApplyTheme(ThemeService.GetTheme(DataPersistService.Get().Theme));

    var mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();

    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      var shutdownMode = OperatingSystem.IsMacOS() ? ShutdownMode.OnExplicitShutdown : ShutdownMode.OnLastWindowClose;

      _mainWindow = new MainWindow { DataContext = mainViewModel };
      mainViewModel.SetMainWindow(_mainWindow);

      desktop.ShutdownMode = shutdownMode;
      desktop.MainWindow = _mainWindow;

      if (OperatingSystem.IsMacOS())
      {
        _mainWindow.Closing += (_, e) =>
        {
          if (e.CloseReason is WindowCloseReason.ApplicationShutdown or WindowCloseReason.OSShutdown) return;

          e.Cancel = true;
          _mainWindow.Hide();
        };

        if (Current?.TryGetFeature<IActivatableLifetime>(out var activatable) == true) activatable.Activated += (_, _) => { _mainWindow.Show(); };
      }

      if (OperatingSystem.IsWindows()) _singleInstanceService?.StartActivateListener(_mainWindow);
    }

    if (OperatingSystem.IsWindows()) SetTrayIcon();

    base.OnFrameworkInitializationCompleted();
  }

  private void SetTrayIcon()
  {
    var quitMenuItem = new NativeMenuItem(Assets.Culture.Resources.QuitApp);
    quitMenuItem.Click += QuitApp_Click;

    var settingsMenuItem = new NativeMenuItem($"{Assets.Culture.Resources.Version}: {ConfigurationService.AppMetadata.AppVersion}");

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
      Menu = [settingsMenuItem, new NativeMenuItemSeparator(), quitMenuItem]
    };

    trayIcon.Clicked += TrayIcon_Click;
    var icons = new TrayIcons { trayIcon };
    TrayIcon.SetIcons(
      Current ?? throw new InvalidOperationException(
        "Application context unavailable."
      ),
      icons
    );
  }

  [SuppressMessage("ReSharper", "AsyncVoidEventHandlerMethod")]
  private async void CheckUpdate_Click(object? sender, EventArgs e)
  {
    if (_mainWindow == null || _updaterService == null) return;

    await _updaterService.CheckForUpdate(_mainWindow);
  }

  [SuppressMessage("ReSharper", "AsyncVoidEventHandlerMethod")]
  private async void AboutDeveloper_Click(object? sender, EventArgs e)
  {
    if (_mainWindow == null) return;

    await UriService.OpenUri(ConfigurationService.AppSettings.HomepageUrl, _mainWindow);
  }

  [SuppressMessage("ReSharper", "AsyncVoidEventHandlerMethod")]
  private async void AboutApp_Click(object? sender, EventArgs e)
  {
    if (_mainWindow == null) return;

    await UriService.OpenUri(ConfigurationService.AppSettings.AppRepoUrl, _mainWindow);
  }

  private static void QuitApp_Click(object? sender, EventArgs e)
  {
    ApplicationService.CloseApplication();
  }

  private void TrayIcon_Click(object? sender, EventArgs e)
  {
    _singleInstanceService?.BringWindowToFront();
  }
}

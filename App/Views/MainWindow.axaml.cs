using System;
using App.Services.Culture;
using App.Services.Theme;
using App.Services.Updater;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Microsoft.Extensions.DependencyInjection;

namespace App.Views;

public sealed partial class MainWindow : Window
{
  private readonly CultureService _cultureService;
  private readonly ThemeService _themeService;
  private readonly UpdaterService _updaterService;

  public MainWindow() : this(GetService<CultureService>(), GetService<ThemeService>(), GetService<UpdaterService>())
  {
  }

  private MainWindow(CultureService cultureService, ThemeService themeService, UpdaterService updaterService)
  {
    InitializeComponent();
    LoadSettings();
    _cultureService = cultureService;
    _themeService = themeService;
    _updaterService = updaterService;
  }

  private void LoadSettings()
  {
    if (OperatingSystem.IsWindows())
    {
      ExtendClientAreaChromeHints =
        ExtendClientAreaChromeHints.NoChrome;
      ExtendClientAreaToDecorationsHint = true;
      MainView.RowDefinitions = new RowDefinitions("35,*");
      MainContent.SetValue(Grid.RowProperty, 1);
      WindowsMenu.IsVisible = true;
    }
    else
    {
      WindowsMenu.IsVisible = false;
    }
  }

  private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
  {
    if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e);
  }

  private void Minimize_Click(object? sender, RoutedEventArgs e)
  {
    WindowState = WindowState.Minimized;
  }

  private void Resize_Click(object? sender, RoutedEventArgs e)
  {
    WindowState = WindowState == WindowState.Maximized
      ? WindowState.Normal
      : WindowState.Maximized;
  }

  private void Close_Click(object? sender, RoutedEventArgs e)
  {
    Close();
  }

  private void ChangeCultureJp_Click(object? sender, RoutedEventArgs e)
  {
    _cultureService.SetCulture(Culture.Jp);
  }

  private void ChangeCultureEn_Click(object? sender, RoutedEventArgs e)
  {
    _cultureService.SetCulture(Culture.En);
  }

  private async void UpdateCheck_Click(object? sender, RoutedEventArgs e)
  {
    await _updaterService.CheckForUpdate(this);
  }

  private void ChangeThemeDark_Click(object? sender, RoutedEventArgs e)
  {
    _themeService.SetTheme(Services.Theme.Theme.Dark);
  }

  private void ChangeThemeSystem_Click(object? sender, RoutedEventArgs e)
  {
    _themeService.SetTheme(Services.Theme.Theme.System);
  }

  private void ChangeThemeLight_Light(object? sender, RoutedEventArgs e)
  {
    _themeService.SetTheme(Services.Theme.Theme.Light);
  }

  private static T GetService<T>() where T : class
  {
    return App.ServiceProvider?.GetRequiredService<T>()
           ?? throw new InvalidOperationException(
             $"Service {typeof(T).Name} not registered");
  }
}

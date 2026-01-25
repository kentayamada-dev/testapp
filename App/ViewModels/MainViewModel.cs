using System.Threading.Tasks;
using App.Services.Culture;
using App.Services.Theme;
using App.Services.Updater;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace App.ViewModels;

public partial class MainViewModel(
  CultureService cultureService,
  ThemeService themeService,
  UpdaterService updaterService)
  : ObservableObject
{
  private Window? _mainWindow;

  [ObservableProperty] private string _darkTheme = Theme.Dark.Value;
  [ObservableProperty] private string _enCode = Culture.En.Code;
  [ObservableProperty] private string _jaCode = Culture.Ja.Code;
  [ObservableProperty] private string _lightTheme = Theme.Light.Value;
  [ObservableProperty] private string _systemTheme = Theme.System.Value;

  public void SetMainWindow(Window mainWindow)
  {
    _mainWindow = mainWindow;
  }

  [RelayCommand]
  private void ChangeCulture(string? culture)
  {
    cultureService.SetCulture(CultureService.GetCulture(culture));
  }

  [RelayCommand]
  private async Task UpdateCheck()
  {
    if (_mainWindow == null) return;

    await updaterService.CheckForUpdate(_mainWindow);
  }

  [RelayCommand]
  private void ChangeTheme(string? theme)
  {
    themeService.SetTheme(ThemeService.GetTheme(theme));
  }
}

using System;
using System.Globalization;
using System.Threading.Tasks;
using App.Assets.Culture;
using App.Services.Application;
using App.Services.Configuration;
using App.Services.Culture;
using App.Services.Data;
using App.Services.Theme;
using App.Services.Updater;
using App.Services.Uri;
using App.Views;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace App.ViewModels;

public partial class MainViewModel(
  IServiceProvider serviceProvider,
  UpdaterService updaterService,
  CaptureFormViewModel captureFormViewModel,
  UploadFormViewModel uploadFormViewModel) : ViewModelBase
{
  [ObservableProperty] private string _appRepoUrl = ConfigurationService.AppSettings.AppRepoUrl;
  [ObservableProperty] private CaptureFormViewModel _captureFormViewModel = captureFormViewModel;
  [ObservableProperty] private string _darkTheme = Theme.Dark.Value;
  [ObservableProperty] private string _enCode = Culture.En.Code;
  [ObservableProperty] private string _homepageUrl = ConfigurationService.AppSettings.HomepageUrl;
  [ObservableProperty] private string _jaCode = Culture.Ja.Code;
  [ObservableProperty] private string _lightTheme = Theme.Light.Value;
  [ObservableProperty] private string _systemTheme = Theme.System.Value;
  [ObservableProperty] private UploadFormViewModel _uploadFormViewModel = uploadFormViewModel;

  private Window? _mainWindow;

  public void SetMainWindow(Window mainWindow)
  {
    _mainWindow = mainWindow;
    CaptureFormViewModel.SetMainWindow(mainWindow);
    UploadFormViewModel.SetMainWindow(mainWindow);
  }

  [RelayCommand]
  private async Task ChangeCulture(string? culture)
  {
    var newCulture = CultureService.GetCulture(culture);

    if (CultureInfo.CurrentCulture.Name == newCulture.Code || _mainWindow == null) return;

    CultureService.ApplyCulture(newCulture);

    await DataPersistService.Update(data => { data.Culture = newCulture.Code; });

    var confirmDialog = serviceProvider.GetRequiredService<ConfirmDialog>();
    confirmDialog.SetMessage(Resources.RestartApp);
    var result = await confirmDialog.ShowDialog<ConfirmDialogResult>(_mainWindow);

    if (result != ConfirmDialogResult.Yes) return;

    ApplicationService.RestartApplication();
  }

  [RelayCommand]
  private async Task UpdateCheck()
  {
    if (_mainWindow == null) return;

    await updaterService.CheckForUpdate(_mainWindow);
  }

  [RelayCommand]
  private async Task OpenUrl(string? url)
  {
    if (_mainWindow == null) return;

    await UriService.OpenUri(url, _mainWindow);
  }

  [RelayCommand]
  private static async Task ChangeTheme(string? theme)
  {
    await ThemeService.ApplyAndSaveTheme(ThemeService.GetTheme(theme));
  }
}

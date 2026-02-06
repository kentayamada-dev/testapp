using System;
using System.Threading.Tasks;
using App.Assets.Culture;
using App.Models;
using App.Services.Data;
using App.Services.FileStorage;
using App.Views;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace App.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
  private readonly IServiceProvider _serviceProvider;
  private Window? _mainWindow;

  [ObservableProperty] private TextInputWithButtonViewModel _filePathField;
  [ObservableProperty] private TextInputFieldViewModel _googleSheetId = new(Resources.RequiredError, watermark: Resources.GoogleSheetId);
  [ObservableProperty] private TextInputFieldViewModel _googleSheetName = new(Resources.RequiredError, watermark: Resources.GoogleSheetName);
  [ObservableProperty] private bool _isUploading;

  public SettingsViewModel(IServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider;
    _filePathField = new TextInputWithButtonViewModel(
      buttonCommand: SelectFileCommand,
      buttonLabel: Resources.Browse,
      errorMessage: Resources.PathError,
      watermark: Resources.EnterGoogleSecretFilePath
    );

    var form = DataPersistService.Get().GoogleForm;

    if (form == null) return;

    FilePathField.Value = form.ClientSecretFile ?? FilePathField.Value;
    GoogleSheetId.Value = form.GoogleSheetId ?? GoogleSheetId.Value;
    GoogleSheetName.Value = form.GoogleSheetName ?? GoogleSheetName.Value;
  }

  public void SetMainWindow(Window mainWindow)
  {
    _mainWindow = mainWindow;
  }

  partial void OnIsUploadingChanged(bool value)
  {
    FilePathField.IsDisabled = value;
    GoogleSheetId.IsDisabled = value;
    GoogleSheetName.IsDisabled = value;
  }

  [RelayCommand]
  private async Task SelectFile()
  {
    var selectedFilePath = await FileStorageService.GetSelectedFilePath(_mainWindow, [FilePickerFileTypes.Json]);

    if (selectedFilePath != null) FilePathField.Value = selectedFilePath;
  }

  [RelayCommand]
  private async Task RegisterGoogleSheetData()
  {
    if (_mainWindow == null) return;

    GoogleSheetId.IsInvalid = string.IsNullOrWhiteSpace(GoogleSheetId.Value);

    GoogleSheetName.IsInvalid = string.IsNullOrWhiteSpace(GoogleSheetName.Value);

    if (GoogleSheetName.IsInvalid || GoogleSheetId.IsInvalid) return;

    await DataPersistService.Update(data =>
    {
      data.GoogleForm ??= new GoogleFormModel();
      data.GoogleForm.GoogleSheetName = GoogleSheetName.Value;
      data.GoogleForm.GoogleSheetId = GoogleSheetId.Value;
    });

    await ShowMessageDialog(_mainWindow, Resources.RegistrationComplete);
  }

  [RelayCommand]
  private async Task RegisterGoogleSecretFile()
  {
    if (_mainWindow == null) return;

    if (string.IsNullOrWhiteSpace(FilePathField.Value) || !await FileStorageService.IsValidFilePath(_mainWindow, FilePathField.Value))
    {
      FilePathField.IsInvalid = true;
      return;
    }

    FilePathField.IsInvalid = false;

    await DataPersistService.Update(data =>
    {
      data.GoogleForm ??= new GoogleFormModel();
      data.GoogleForm.ClientSecretFile = FilePathField.Value;
    });

    await ShowMessageDialog(_mainWindow, Resources.RegistrationComplete);
  }

  private async Task ShowMessageDialog(Window owner, string message)
  {
    var dialog = _serviceProvider.GetRequiredService<MessageDialog>();
    dialog.SetMessage(message);
    dialog.SetShowCancelButton(false);
    await dialog.ShowDialog<MessageDialogResult>(owner);
  }
}

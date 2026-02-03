using System.Threading.Tasks;
using App.Assets.Culture;
using App.Models;
using App.Services.Data;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace App.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
  [ObservableProperty] private TextInputWithButtonViewModel _filePathField;
  [ObservableProperty] private TextInputFieldViewModel _googleSheetId = new(Resources.RequiredError, watermark: Resources.GoogleSheetId);
  [ObservableProperty] private TextInputFieldViewModel _googleSheetName = new(Resources.RequiredError, watermark: Resources.GoogleSheetName);
  [ObservableProperty] private bool _isUploading;

  private Window? _mainWindow;

  public SettingsViewModel()
  {
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
    var topLevel = TopLevel.GetTopLevel(_mainWindow);

    if (topLevel == null) return;

    var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
    {
      FileTypeFilter = [new FilePickerFileType(null) { Patterns = ["*.json"] }]
    });

    if (files.Count > 0) FilePathField.Value = files[0].Path.LocalPath;
  }

  [RelayCommand]
  private async Task RegisterGoogleSheetData()
  {
    GoogleSheetId.IsInvalid = string.IsNullOrWhiteSpace(GoogleSheetId.Value);

    GoogleSheetName.IsInvalid = string.IsNullOrWhiteSpace(GoogleSheetName.Value);

    if (GoogleSheetName.IsInvalid || GoogleSheetId.IsInvalid) return;

    await DataPersistService.Update(data =>
    {
      data.GoogleForm ??= new GoogleFormModel();
      data.GoogleForm.GoogleSheetName = GoogleSheetName.Value;
      data.GoogleForm.GoogleSheetId = GoogleSheetId.Value;
    });
  }

  [RelayCommand]
  private async Task RegisterGoogleSecretFile()
  {
    if (string.IsNullOrWhiteSpace(FilePathField.Value) || !await IsValidFilePath(FilePathField.Value))
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
  }

  private async Task<bool> IsValidFilePath(string? filePath)
  {
    if (_mainWindow == null || filePath == null) return false;

    var file = await _mainWindow.StorageProvider.TryGetFileFromPathAsync(filePath);

    return file != null;
  }
}

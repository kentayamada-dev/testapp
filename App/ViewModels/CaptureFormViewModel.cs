using System.Threading.Tasks;
using App.Assets.Culture;
using App.Services.Data;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace App.ViewModels;

public partial class CaptureFormViewModel : ViewModelBase
{
  [ObservableProperty] private TextInputFieldViewModel _intervalField = new()
  {
    Label = Resources.Interval,
    ErrorMessage = Resources.IntervalError
  };

  [ObservableProperty] private TextInputFieldViewModel _outputFolderField = new()
  {
    Label = Resources.OutputFolder,
    ErrorMessage = Resources.OutputFolderError
  };

  [ObservableProperty] private TextInputFieldViewModel _urlField = new()
  {
    Label = "URL",
    ErrorMessage = Resources.UrlError
  };

  private Window? _mainWindow;

  public CaptureFormViewModel()
  {
    var form = DataPersistService.Get().CaptureForm;
    UrlField.Value = form.Url ?? "";
    IntervalField.Value = form.Interval ?? "";
    OutputFolderField.Value = form.OutputFolder ?? "";
  }

  public void SetMainWindow(Window mainWindow)
  {
    _mainWindow = mainWindow;
  }

  [RelayCommand]
  private async Task SelectFolder()
  {
    if (_mainWindow == null) return;

    var folders = await _mainWindow.StorageProvider.OpenFolderPickerAsync(
      new FolderPickerOpenOptions());

    if (folders.Count > 0) OutputFolderField.Value = folders[0].Path.LocalPath;
  }

  [RelayCommand]
  private async Task Capture()
  {
    if (await IsInValid()) return;

    await DataPersistService.Update(data =>
    {
      data.CaptureForm.Url = UrlField.Value;
      data.CaptureForm.Interval = IntervalField.Value;
      data.CaptureForm.OutputFolder = OutputFolderField.Value;
    });
  }

  private async Task<bool> IsInValid()
  {
    if (string.IsNullOrWhiteSpace(UrlField.Value))
      UrlField.IsInvalid = true;
    else
      UrlField.IsInvalid = false;

    if (string.IsNullOrWhiteSpace(OutputFolderField.Value) || !await IsValidFolderPath(OutputFolderField.Value))
      OutputFolderField.IsInvalid = true;
    else
      OutputFolderField.IsInvalid = false;

    if (string.IsNullOrWhiteSpace(IntervalField.Value) || !decimal.TryParse(IntervalField.Value, out _))
      IntervalField.IsInvalid = true;
    else
      IntervalField.IsInvalid = false;

    return IntervalField.IsInvalid || OutputFolderField.IsInvalid || UrlField.IsInvalid;
  }

  private async Task<bool> IsValidFolderPath(
    string? folderPath)
  {
    if (_mainWindow == null || folderPath == null) return false;

    var folder = await _mainWindow.StorageProvider.TryGetFolderFromPathAsync(folderPath);

    return folder != null;
  }
}

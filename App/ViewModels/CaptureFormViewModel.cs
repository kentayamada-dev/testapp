using System;
using System.Threading;
using System.Threading.Tasks;
using App.Assets.Culture;
using App.Services.Capture;
using App.Services.Data;
using App.Services.Logger;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace App.ViewModels;

public partial class CaptureFormViewModel : ViewModelBase
{
  private const string StopIconPath = "M0,0 L56,0 L56,56 L0,56 z";
  private const string PlayIconPath = "M0,0 L56,28 L0,56 z";

  private Window? _mainWindow;

  [ObservableProperty] private CancellationTokenSource? _captureCancellationTokenSource;

  [ObservableProperty] private string _capturePathData = PlayIconPath;

  [ObservableProperty] private TextInputFieldViewModel _intervalField = new()
  {
    Label = Resources.Interval,
    ErrorMessage = Resources.IntervalError
  };

  [ObservableProperty] private bool _isCapturing;

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

  partial void OnCaptureCancellationTokenSourceChanged(CancellationTokenSource? value)
  {
    CapturePathData = value == null ? PlayIconPath : StopIconPath;
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
    if (CaptureCancellationTokenSource != null)
    {
      await CaptureCancellationTokenSource.CancelAsync();
      CaptureCancellationTokenSource = null;
      return;
    }

    if (await IsInValid()) return;

    await DataPersistService.Update(data =>
    {
      data.CaptureForm.Url = UrlField.Value;
      data.CaptureForm.Interval = IntervalField.Value;
      data.CaptureForm.OutputFolder = OutputFolderField.Value;
    });

    CaptureCancellationTokenSource = new CancellationTokenSource();
    _ = CaptureAsync(int.Parse(IntervalField.Value), CaptureCancellationTokenSource.Token);
  }

  private async Task CaptureAsync(int interval, CancellationToken cancellationToken)
  {
    try
    {
      while (!cancellationToken.IsCancellationRequested)
      {
        await Task.Delay(TimeSpan.FromSeconds(interval), cancellationToken);

        await CaptureService.Capture(
          UrlField.Value,
          OutputFolderField.Value);
      }
    }
    catch (Exception ex)
    {
      await LoggerService.Log(ex);
    }
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

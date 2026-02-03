using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using App.Assets.Culture;
using App.Models;
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
  private readonly SemaphoreSlim _captureSemaphore = new(1, 1);

  [ObservableProperty] private string _buttonIconData = PlayIconPath;
  [ObservableProperty] private CancellationTokenSource? _cancellationTokenSource;
  [ObservableProperty] private TextInputFieldViewModel _intervalField = new(Resources.CaptureIntervalError, Resources.CaptureInterval);
  [ObservableProperty] private TextInputWithButtonViewModel _outputFolderField;
  [ObservableProperty] private TextInputFieldViewModel _urlField = new(Resources.RequiredError, "URL");

  public CaptureFormViewModel()
  {
    _outputFolderField = new TextInputWithButtonViewModel(
      buttonCommand: SelectFolderCommand,
      fieldLabel: Resources.OutputFolder,
      buttonLabel: Resources.Browse,
      errorMessage: Resources.PathError,
      watermark: Resources.EnterGoogleSecretFilePath
    );

    var form = DataPersistService.Get().CaptureForm;

    if (form == null) return;

    UrlField.Value = form.Url ?? UrlField.Value;
    IntervalField.Value = form.Interval ?? IntervalField.Value;
    OutputFolderField.Value = form.OutputFolder ?? OutputFolderField.Value;
  }

  public void SetMainWindow(Window mainWindow)
  {
    _mainWindow = mainWindow;
  }

  partial void OnCancellationTokenSourceChanged(CancellationTokenSource? value)
  {
    ButtonIconData = value == null ? PlayIconPath : StopIconPath;
    IntervalField.IsDisabled = value != null;
    UrlField.IsDisabled = value != null;
    OutputFolderField.IsDisabled = value != null;
  }

  [RelayCommand]
  private async Task SelectFolder()
  {
    if (_mainWindow == null) return;

    var folders = await _mainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions());

    if (folders.Count > 0) OutputFolderField.Value = folders[0].Path.LocalPath;
  }

  [RelayCommand]
  private async Task Capture()
  {
    if (CancellationTokenSource != null)
    {
      await CancellationTokenSource.CancelAsync();
      CancellationTokenSource = null;
      return;
    }

    if (await IsInValid()) return;

    await DataPersistService.Update(data =>
    {
      data.CaptureForm ??= new CaptureFormModel();
      data.CaptureForm.Url = UrlField.Value;
      data.CaptureForm.Interval = IntervalField.Value;
      data.CaptureForm.OutputFolder = OutputFolderField.Value;
    });

    CancellationTokenSource = new CancellationTokenSource();
    _ = CaptureAsync(byte.Parse(IntervalField.Value), CancellationTokenSource.Token);
  }

  private async Task CaptureAsync(int interval, CancellationToken cancellationToken)
  {
    try
    {
      while (!cancellationToken.IsCancellationRequested)
      {
        await Task.Delay(TimeSpan.FromSeconds(interval), cancellationToken);

        await _captureSemaphore.WaitAsync(cancellationToken);
        try
        {
          await CaptureService.Capture(
            UrlField.Value,
            OutputFolderField.Value);
        }
        finally
        {
          _captureSemaphore.Release();
        }
      }
    }
    catch (Exception ex)
    {
      await LoggerService.Log(ex);
    }
  }

  private async Task<bool> IsInValid()
  {
    UrlField.IsInvalid = string.IsNullOrWhiteSpace(UrlField.Value);

    if (string.IsNullOrWhiteSpace(OutputFolderField.Value) || !await IsValidFolderPath(OutputFolderField.Value))
      OutputFolderField.IsInvalid = true;
    else
      OutputFolderField.IsInvalid = false;

    if (string.IsNullOrWhiteSpace(IntervalField.Value) || !byte.TryParse(IntervalField.Value, NumberStyles.None, null, out var result) || result < 1)
      IntervalField.IsInvalid = true;
    else
      IntervalField.IsInvalid = false;

    return IntervalField.IsInvalid || OutputFolderField.IsInvalid || UrlField.IsInvalid;
  }

  private async Task<bool> IsValidFolderPath(string? folderPath)
  {
    if (_mainWindow == null || folderPath == null) return false;

    var folder = await _mainWindow.StorageProvider.TryGetFolderFromPathAsync(folderPath);

    return folder != null;
  }
}

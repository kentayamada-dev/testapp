using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using App.Assets.Culture;
using App.Models;
using App.Services.Capture;
using App.Services.Data;
using App.Services.FileStorage;
using App.Services.Logger;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace App.ViewModels;

public partial class CaptureFormViewModel : ViewModelBase
{
  private const string StopIconPath = "M0,0 L56,0 L56,56 L0,56 z";
  private const string PlayIconPath = "M0,0 L56,28 L0,56 z";
  private readonly CaptureService _captureService;
  private readonly LoggerService _loggerService;
  private Window? _mainWindow;

  [ObservableProperty] private string _buttonIconData = PlayIconPath;
  [ObservableProperty] private CancellationTokenSource? _cancellationTokenSource;
  [ObservableProperty] private TextInputFieldViewModel _intervalField = new(Resources.CaptureIntervalError, Resources.CaptureInterval);
  [ObservableProperty] private TextInputWithButtonViewModel _outputFolderField;
  [ObservableProperty] private TextInputFieldViewModel _urlField = new(Resources.RequiredError, "URL");

  public CaptureFormViewModel(LoggerService loggerService, CaptureService captureService)
  {
    _captureService = captureService;
    _loggerService = loggerService;
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
    var selectedFolder = await FileStorageService.GetSelectedFolderPath(_mainWindow);

    if (selectedFolder != null) OutputFolderField.Value = selectedFolder;
  }

  [RelayCommand]
  private async Task Capture()
  {
    if (CancellationTokenSource != null)
    {
      await CancellationTokenSource.CancelAsync();
      CancellationTokenSource.Dispose();
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
        await Task.Delay(TimeSpan.FromMinutes(interval), cancellationToken);

        await _captureService.Capture(TimeSpan.FromSeconds(5), UrlField.Value, OutputFolderField.Value, DateTime.Now);
      }
    }
    catch (Exception ex)
    {
      await _loggerService.Log(ex);
    }
  }

  private async Task<bool> IsInValid()
  {
    UrlField.IsInvalid = string.IsNullOrWhiteSpace(UrlField.Value);

    if (string.IsNullOrWhiteSpace(OutputFolderField.Value) || !await FileStorageService.IsValidFolderPath(_mainWindow, OutputFolderField.Value))
      OutputFolderField.IsInvalid = true;
    else
      OutputFolderField.IsInvalid = false;

    if (string.IsNullOrWhiteSpace(IntervalField.Value) || !byte.TryParse(IntervalField.Value, NumberStyles.None, null, out var result) || result < 1)
      IntervalField.IsInvalid = true;
    else
      IntervalField.IsInvalid = false;

    return IntervalField.IsInvalid || OutputFolderField.IsInvalid || UrlField.IsInvalid;
  }
}

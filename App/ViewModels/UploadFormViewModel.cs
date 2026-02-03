using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using App.Assets.Culture;
using App.Models;
using App.Services.Capture;
using App.Services.Data;
using App.Services.Google;
using App.Services.Logger;
using App.Views;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace App.ViewModels;

public class ScheduleOption(string label, byte id)
{
  public byte Id { get; } = id;

  public override string ToString()
  {
    return label;
  }
}

public static class ScheduleOptions
{
  public static readonly ScheduleOption[] Options =
  [
    new(Resources.UploadInterval_1, 1),
    new(Resources.UploadInterval_2, 2),
    new(Resources.UploadInterval_3, 3),
    new(Resources.UploadInterval_4, 4)
  ];
}

public partial class UploadFormViewModel : ViewModelBase
{
  private const string StopIconPath = "M0,0 L56,0 L56,56 L0,56 z";
  private const string PlayIconPath = "M0,0 L56,28 L0,56 z";
  private Window? _mainWindow;
  private readonly SemaphoreSlim _captureSemaphore = new(1, 1);
  private readonly IServiceProvider _serviceProvider;

  [ObservableProperty] private string _buttonIconData = PlayIconPath;
  [ObservableProperty] private CancellationTokenSource? _cancellationTokenSource;
  [ObservableProperty] private TextInputFieldViewModel _fpsField = new(Resources.RequiredError, "FPS");
  [ObservableProperty] private TextInputWithButtonViewModel _inputFolderField;
  [ObservableProperty] private ComboBoxFieldUploadIntervalViewModel _intervalField = new(ScheduleOptions.Options[0], new ObservableCollection<ScheduleOption>(ScheduleOptions.Options), Resources.UploadInterval);
  [ObservableProperty] private SettingsViewModel _settingsViewModel;

  public UploadFormViewModel(IServiceProvider serviceProvider, SettingsViewModel settingsViewModel)
  {
    _settingsViewModel = settingsViewModel;
    _serviceProvider = serviceProvider;
    _inputFolderField = new TextInputWithButtonViewModel(
      buttonCommand: SelectFolderCommand,
      fieldLabel: Resources.InputFolder,
      buttonLabel: Resources.Browse,
      errorMessage: Resources.PathError,
      watermark: Resources.EnterGoogleSecretFilePath
    );

    var form = DataPersistService.Get().UploadForm;

    if (form == null) return;

    FpsField.Value = form.Fps ?? FpsField.Value;
    InputFolderField.Value = form.InputFolder ?? InputFolderField.Value;
    if (byte.TryParse(form.IntervalId, out var intervalId))
      IntervalField.SelectedOption =
        Array.Find(ScheduleOptions.Options,
          option => option.Id == intervalId) ?? IntervalField.SelectedOption;
  }

  public void SetMainWindow(Window mainWindow)
  {
    _mainWindow = mainWindow;
  }

  partial void OnCancellationTokenSourceChanged(CancellationTokenSource? value)
  {
    ButtonIconData = value == null ? PlayIconPath : StopIconPath;
    FpsField.IsDisabled = value != null;
    InputFolderField.IsDisabled = value != null;
    IntervalField.IsDisabled = value != null;
    _settingsViewModel.IsUploading = value != null;
  }

  [RelayCommand]
  private async Task SelectFolder()
  {
    if (_mainWindow == null) return;

    var folders = await _mainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions());

    if (folders.Count > 0) InputFolderField.Value = folders[0].Path.LocalPath;
  }

  [RelayCommand]
  private async Task Upload()
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
      data.UploadForm ??= new UploadFormModel();
      data.UploadForm.Fps = FpsField.Value;
      data.UploadForm.InputFolder = InputFolderField.Value;
      data.UploadForm.IntervalId = IntervalField.SelectedOption.Id.ToString();
    });

    CancellationTokenSource = new CancellationTokenSource();
    _ = UploadAsync(byte.Parse(FpsField.Value), IntervalField.SelectedOption.Id, InputFolderField.Value, CancellationTokenSource.Token);
  }

  private async Task UploadAsync(byte fps, byte scheduleOptionId, string inputFolder, CancellationToken cancellationToken)
  {
    var form = DataPersistService.Get().GoogleForm;

    if (form?.ClientSecretFile == null || form?.GoogleSheetId == null || form?.GoogleSheetName == null)
    {
      if (_mainWindow == null || CancellationTokenSource == null) return;

      var dialog = _serviceProvider.GetRequiredService<MessageDialog>();
      dialog.SetMessage(Resources.RegisterGoogleDataWarning);
      dialog.SetShowCancelButton(false);
      await dialog.ShowDialog<MessageDialogResult>(_mainWindow);
      await CancellationTokenSource.CancelAsync();
      CancellationTokenSource = null;
      return;
    }

    var scheduleHours = GetScheduleHours(scheduleOptionId);
    var videoFilePath = Path.Join(inputFolder, "output.mp4");

    try
    {
      await GoogleService.Initialize(form.ClientSecretFile);

      while (!cancellationToken.IsCancellationRequested)
      {
        var now = DateTime.Now;
        var nextRun = GetNextScheduledTime(now, scheduleHours);
        var delayUntilNext = nextRun - now;

        await Task.Delay(delayUntilNext, cancellationToken);

        await _captureSemaphore.WaitAsync(cancellationToken);
        try
        {
          var imageFiles = Directory.GetFiles(inputFolder, "*.png", SearchOption.AllDirectories).OrderBy(file => file).ToList();
          var firstCreatedTime = new FileInfo(imageFiles.First()).CreationTime.ToString("yyyy/MM/dd HH:mm:ss");
          var lastCreatedTime = new FileInfo(imageFiles.Last()).CreationTime.ToString("yyyy/MM/dd HH:mm:ss");

          await CaptureService.CreateVideo(imageFiles, videoFilePath, fps);

          foreach (var file in imageFiles) File.Delete(file);
          var allDirs = Directory.GetDirectories(inputFolder, "*", SearchOption.AllDirectories).OrderByDescending(d => d.Length).ToList();
          foreach (var dir in allDirs.Where(dir => Directory.GetFileSystemEntries(dir).Length == 0)) Directory.Delete(dir);

          var videoId = await GoogleService.UploadVideo(
            videoFilePath,
            $"{firstCreatedTime} ~ {lastCreatedTime}",
            progress => { _ = LoggerService.Log($@"Uploaded: {progress}%"); }
          );

          await LoggerService.Log($"Video ID: {videoId}");

          await GoogleService.AppendDataToSheet(
            form.GoogleSheetId,
            form.GoogleSheetName,
            new List<IList<object>>
            {
              new List<object> { now, $"https://youtu.be/{videoId}" }
            }
          );
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

  private static int[] GetScheduleHours(int scheduleOptionId)
  {
    return scheduleOptionId switch
    {
      1 => [0],
      2 => [0, 12],
      3 => [0, 8, 16],
      4 => [0, 6, 12, 18],
      _ => [0]
    };
  }

  private static DateTime GetNextScheduledTime(
    DateTime now,
    int[] scheduleHours)
  {
    var today = now.Date;

    foreach (var hour in scheduleHours.OrderBy(h => h))
    {
      var scheduledTime = today.AddHours(hour);
      if (scheduledTime > now) return scheduledTime;
    }

    return today.AddDays(1).AddHours(scheduleHours[0]);
  }

  private async Task<bool> IsInValid()
  {
    if (string.IsNullOrWhiteSpace(InputFolderField.Value) || !await IsValidFolderPath(InputFolderField.Value))
      InputFolderField.IsInvalid = true;
    else
      InputFolderField.IsInvalid = false;

    if (string.IsNullOrWhiteSpace(FpsField.Value) || !byte.TryParse(FpsField.Value, NumberStyles.None, null, out var result) || result < 1)
      FpsField.IsInvalid = true;
    else
      FpsField.IsInvalid = false;

    return FpsField.IsInvalid || InputFolderField.IsInvalid;
  }

  private async Task<bool> IsValidFolderPath(string? folderPath)
  {
    if (_mainWindow == null || folderPath == null) return false;

    var folder = await _mainWindow.StorageProvider.TryGetFolderFromPathAsync(folderPath);

    return folder != null;
  }
}

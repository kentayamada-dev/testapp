using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using App.Assets.Culture;
using App.Models;
using App.Services.Data;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

  [ObservableProperty] private string _buttonIconData = PlayIconPath;

  [ObservableProperty] private CancellationTokenSource? _cancellationTokenSource;

  [ObservableProperty] private TextInputFieldViewModel _fpsField = new(
    "FPS",
    Resources.FpsError
  );

  [ObservableProperty] private TextInputFieldViewModel _inputFolderField = new(
    Resources.InputFolder,
    Resources.FolderError);

  [ObservableProperty] private ComboBoxFieldUploadIntervalViewModel _intervalField = new(
    label: Resources.UploadInterval,
    options: new ObservableCollection<ScheduleOption>(ScheduleOptions.Options),
    selectedOption: ScheduleOptions.Options[0]
  );

  private Window? _mainWindow;

  public UploadFormViewModel()
  {
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
    _ = UploadAsync(byte.Parse(FpsField.Value), CancellationTokenSource.Token);
  }

  private async Task UploadAsync(byte fps, CancellationToken cancellationToken)
  {
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

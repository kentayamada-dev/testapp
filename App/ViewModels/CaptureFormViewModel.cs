using System.Threading.Tasks;
using App.Assets.Culture;
using App.Services.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace App.ViewModels;

public partial class CaptureFormViewModel : ViewModelBase
{
  [ObservableProperty] private TextInputFieldViewModel _intervalField = new()
  {
    Label = Resources.Interval,
    ErrorMessage = "Interval must be a valid decimal number"
  };

  [ObservableProperty] private TextInputFieldViewModel _outputFolderField = new()
  {
    Label = Resources.OutputFolder,
    ErrorMessage = "Output folder path is required"
  };

  [ObservableProperty] private TextInputFieldViewModel _urlField = new()
  {
    Label = "URL",
    ErrorMessage = "URL is required"
  };

  public CaptureFormViewModel()
  {
    var form = DataPersistService.Get().CaptureForm;
    UrlField.Value = form.Url ?? "";
    IntervalField.Value = form.Interval ?? "";
    OutputFolderField.Value = form.OutputFolder ?? "";
  }

  [RelayCommand]
  private async Task Capture()
  {
    if (IsInValid()) return;

    await DataPersistService.Update(data =>
    {
      data.CaptureForm.Url = UrlField.Value;
      data.CaptureForm.Interval = IntervalField.Value;
      data.CaptureForm.OutputFolder = OutputFolderField.Value;
    });
  }

  private bool IsInValid()
  {
    if (string.IsNullOrWhiteSpace(UrlField.Value))
      UrlField.IsInvalid = true;
    else
      UrlField.IsInvalid = false;

    if (string.IsNullOrWhiteSpace(OutputFolderField.Value))
      OutputFolderField.IsInvalid = true;
    else
      OutputFolderField.IsInvalid = false;

    if (string.IsNullOrWhiteSpace(IntervalField.Value) || !decimal.TryParse(IntervalField.Value, out _))
      IntervalField.IsInvalid = true;
    else
      IntervalField.IsInvalid = false;

    return IntervalField.IsInvalid || OutputFolderField.IsInvalid || UrlField.IsInvalid;
  }
}

using CommunityToolkit.Mvvm.ComponentModel;

namespace App.ViewModels;

public partial class TextInputFieldViewModel(string errorMessage, string label = "", bool isInvalid = false, string value = "", string watermark = "")
  : ViewModelBase
{
  [ObservableProperty] private string _errorMessage = errorMessage;
  [ObservableProperty] private bool _isDisabled;
  [ObservableProperty] private bool _isInvalid = isInvalid;
  [ObservableProperty] private bool _isLabelVisible = !string.IsNullOrEmpty(label);
  [ObservableProperty] private string _label = label;
  [ObservableProperty] private string _value = value;
  [ObservableProperty] private string _watermark = watermark;
}

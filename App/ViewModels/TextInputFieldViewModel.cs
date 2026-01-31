using CommunityToolkit.Mvvm.ComponentModel;

namespace App.ViewModels;

public partial class TextInputFieldViewModel(string label, string errorMessage, bool isInvalid = false, string value = "") : ViewModelBase
{
  [ObservableProperty] private string _errorMessage = errorMessage;
  [ObservableProperty] private bool _isInvalid = isInvalid;
  [ObservableProperty] private string _label = label;
  [ObservableProperty] private string _value = value;
}

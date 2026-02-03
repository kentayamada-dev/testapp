using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace App.ViewModels;

public partial class TextInputWithButtonViewModel(
  string errorMessage,
  ICommand buttonCommand,
  string buttonLabel,
  string fieldLabel = "",
  bool isInvalid = false,
  string value = "",
  string watermark = "") : ViewModelBase
{
  [ObservableProperty] private ICommand _buttonCommand = buttonCommand;
  [ObservableProperty] private string _buttonLabel = buttonLabel;
  [ObservableProperty] private string _errorMessage = errorMessage;
  [ObservableProperty] private string _fieldLabel = fieldLabel;
  [ObservableProperty] private bool _isDisabled;
  [ObservableProperty] private bool _isInvalid = isInvalid;
  [ObservableProperty] private bool _isLabelVisible = !string.IsNullOrEmpty(fieldLabel);
  [ObservableProperty] private string _value = value;
  [ObservableProperty] private string _watermark = watermark;
}

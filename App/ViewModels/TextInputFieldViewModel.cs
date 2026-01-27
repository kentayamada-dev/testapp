using CommunityToolkit.Mvvm.ComponentModel;

namespace App.ViewModels;

public partial class TextInputFieldViewModel : ViewModelBase
{
  [ObservableProperty] private string _errorMessage = "";

  [ObservableProperty] private bool _isInvalid;
  [ObservableProperty] private string _label = "";

  [ObservableProperty] private string _value = "";
}

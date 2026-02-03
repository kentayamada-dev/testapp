using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace App.ViewModels;

public partial class ComboBoxFieldViewModel<T>(T selectedOption, ObservableCollection<T> options, string label = "") : ViewModelBase
{
  [ObservableProperty] private bool _isDisabled;
  [ObservableProperty] private bool _isLabelVisible = !string.IsNullOrEmpty(label);
  [ObservableProperty] private string _label = label;
  [ObservableProperty] private ObservableCollection<T> _options = options;
  [ObservableProperty] private T _selectedOption = selectedOption;
}

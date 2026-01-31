using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace App.ViewModels;

public partial class ComboBoxFieldViewModel<T>(T selectedOption, string label, ObservableCollection<T> options) : ViewModelBase
{
  [ObservableProperty] private string _label = label;
  [ObservableProperty] private ObservableCollection<T> _options = options;
  [ObservableProperty] private T _selectedOption = selectedOption;
}

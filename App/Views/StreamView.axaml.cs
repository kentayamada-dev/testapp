using App.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace App.Views;

public partial class StreamView : UserControl
{
  public StreamView()
  {
    InitializeComponent();

    Loaded += (_, _) =>
    {
      if (DataContext is not StreamViewModel viewModel) return;

      viewModel.PlayCommand.Execute(null);
      SoundIcon.Data = viewModel.ButtonIcon;
    };

    Unloaded += (_, _) =>
    {
      if (DataContext is not StreamViewModel viewModel) return;

      viewModel.StopCommand.Execute(null);
    };
  }

  private void ToggleButton(object? sender, RoutedEventArgs e)
  {
    if (DataContext is not StreamViewModel viewModel) return;

    viewModel.ToggleSoundCommand.Execute(null);
    SoundIcon.Data = viewModel.ButtonIcon;
  }
}

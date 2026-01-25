using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace App.Views;

public partial class ChromeMinimizeButton : UserControl
{
  public ChromeMinimizeButton()
  {
    InitializeComponent();
  }

  public event EventHandler<RoutedEventArgs>? RoutedEvent;

  private void Minimize_Click(object? sender, RoutedEventArgs e)
  {
    RoutedEvent?.Invoke(this, e);
  }
}

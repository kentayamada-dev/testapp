using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace App.Views;

public partial class ChromeCloseButton : UserControl
{
  public ChromeCloseButton()
  {
    InitializeComponent();
  }

  public event EventHandler<RoutedEventArgs>? RoutedEvent;

  private void Close_Click(object? sender, RoutedEventArgs e)
  {
    RoutedEvent?.Invoke(this, e);
  }
}

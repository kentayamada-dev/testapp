using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;

namespace App.Views;

public sealed partial class MainWindow : Window
{
  public MainWindow()
  {
    InitializeComponent();
    LoadSettings();
  }

  private void LoadSettings()
  {
    if (OperatingSystem.IsWindows())
    {
      ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
      ExtendClientAreaToDecorationsHint = true;
      MainView.RowDefinitions = new RowDefinitions($"{Application.Current?.Resources["ChromeHeight"]},*");
      MainContent.SetValue(Grid.RowProperty, 1);
      WindowsMenu.IsVisible = true;
    }
    else
    {
      WindowsMenu.IsVisible = false;
    }
  }

  private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
  {
    if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e);
  }

  private void Minimize_Click(object? sender, RoutedEventArgs e)
  {
    WindowState = WindowState.Minimized;
  }

  private void Close_Click(object? sender, RoutedEventArgs e)
  {
    Hide();
  }
}

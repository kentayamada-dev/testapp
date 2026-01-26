using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;

namespace App.Views;

public partial class ProgressDialog : Window
{
  public ProgressDialog()
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
      WindowsMenu.IsVisible = true;
    }
    else
    {
      WindowsMenu.IsVisible = false;
    }
  }

  public void SetProgress(double progress)
  {
    ProgressBar.Value = progress;
  }

  private void Close_Click(object? sender, RoutedEventArgs e)
  {
    Close();
  }

  private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
  {
    if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e);
  }
}

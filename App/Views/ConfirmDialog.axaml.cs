using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;

namespace App.Views;

public enum ConfirmDialogResult
{
  Yes,
  No
}

public partial class ConfirmDialog : Window
{
  public ConfirmDialog()
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

  public void SetMessage(string message)
  {
    MessageText.Text = message;
  }

  private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
  {
    if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e);
  }

  private void Close_Click(object? sender, RoutedEventArgs e)
  {
    Close(ConfirmDialogResult.No);
  }

  private void YesButton_Click(object? sender, RoutedEventArgs e)
  {
    Close(ConfirmDialogResult.Yes);
  }

  private void NoButton_Click(object? sender, RoutedEventArgs e)
  {
    Close(ConfirmDialogResult.No);
  }
}

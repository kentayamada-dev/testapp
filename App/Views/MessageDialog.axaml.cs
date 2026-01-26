using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;

namespace App.Views;

public enum DialogResult
{
  Ok,
  Cancel
}

public partial class MessageDialog : Window
{
  public MessageDialog()
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

  public void SetShowCancelButton(bool show)
  {
    CancelButton.IsVisible = show;
  }

  private void Ok_Click(object? sender, RoutedEventArgs e)
  {
    Close(DialogResult.Ok);
  }

  private void Cancel_Click(object? sender, RoutedEventArgs e)
  {
    Close(DialogResult.Cancel);
  }

  private void Close_Click(object? sender, RoutedEventArgs e)
  {
    Close(DialogResult.Cancel);
  }

  private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
  {
    if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e);
  }
}

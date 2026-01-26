using System.Diagnostics;
using Avalonia.Controls.ApplicationLifetimes;

namespace App.Services.Application;

public static class ApplicationService
{
  public static void RestartApplication()
  {
    if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;

    var exePath = Process.GetCurrentProcess().MainModule?.FileName;

    if (exePath == null) return;

    Process.Start(exePath);
    desktop.Shutdown();
  }

  public static void CloseApplication()
  {
    if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;

    desktop.Shutdown();
  }
}

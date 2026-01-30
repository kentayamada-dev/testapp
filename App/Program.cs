using System;
using App.Services.Application;
using Avalonia;
using Avalonia.Media.Fonts;

namespace App;

public sealed class MyFontCollection() : EmbeddedFontCollection(new Uri("fonts:MyFonts", UriKind.Absolute),
  new Uri("avares://App/Assets/Fonts", UriKind.Absolute));

public static class Program
{
  public static SingleInstanceService? SingleInstanceService { get; private set; }

  [STAThread]
  public static void Main(string[] args)
  {
    if (OperatingSystem.IsWindows())
    {
      var single = new SingleInstanceService();
      single.CheckSingleInstance();

      if (!single.IsNewInstance)
      {
        single.Dispose();
        return;
      }

      SingleInstanceService = single;
    }

    BuildAvaloniaApp()
      .StartWithClassicDesktopLifetime(args);
  }

  private static AppBuilder BuildAvaloniaApp()
  {
    return AppBuilder.Configure<App>()
      .UsePlatformDetect()
      .With(new MacOSPlatformOptions
      {
        DisableDefaultApplicationMenuItems = true
      })
      .ConfigureFonts(fontManager => { fontManager.AddFontCollection(new MyFontCollection()); })
      .LogToTrace();
  }
}

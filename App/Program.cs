using System;
using Avalonia;
using Avalonia.Media.Fonts;

namespace App;

public sealed class MyFontCollection() : EmbeddedFontCollection(new Uri("fonts:MyFonts", UriKind.Absolute),
  new Uri("avares://App/Assets/Fonts", UriKind.Absolute));

public static class Program
{
  [STAThread]
  public static void Main(string[] args)
  {
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

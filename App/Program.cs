using System;
using App.Services.Application;
using App.Services.Configuration;
using Avalonia;
using Avalonia.Media.Fonts;

namespace App;

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
      .ConfigureFonts(fontManager =>
      {
        fontManager.AddFontCollection(
          new EmbeddedFontCollection(
            new Uri("fonts:MyFonts", UriKind.Absolute),
            new Uri($"avares://{ConfigurationService.AppMetadata.AssemblyName}/Assets/Fonts", UriKind.Absolute)
          )
        );
      })
      .LogToTrace();
  }
}

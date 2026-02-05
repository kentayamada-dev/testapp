using System.Threading.Tasks;
using App.Services.Data;
using Avalonia.Styling;

namespace App.Services.Theme;

public static class ThemeService
{
  public static Theme GetTheme(string? theme)
  {
    return Theme.FromValue(theme) ?? Theme.System;
  }

  public static void ApplyTheme(Theme theme)
  {
    var themeVariant = theme.Value switch { "Dark" => ThemeVariant.Dark, "Light" => ThemeVariant.Light, _ => ThemeVariant.Default };

    Avalonia.Application.Current?.RequestedThemeVariant = themeVariant;
  }

  public static async Task ApplyAndSaveTheme(Theme theme)
  {
    ApplyTheme(theme);
    await DataPersistService.Update(data => { data.Theme = theme.Value; });
  }
}

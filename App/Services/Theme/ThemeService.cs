using App.Services.Data;
using Avalonia.Styling;

namespace App.Services.Theme;

public sealed class ThemeService(DataPersistService dataService)
{
  public static Theme GetTheme(string? theme)
  {
    return Theme.FromValue(theme) ?? Theme.System;
  }

  public static void ApplyTheme(Theme theme)
  {
    var themeVariant = theme.Value switch
    {
      "Dark" => ThemeVariant.Dark,
      "Light" => ThemeVariant.Light,
      _ => ThemeVariant.Default
    };

    Avalonia.Application.Current?.RequestedThemeVariant = themeVariant;
  }

  public void ApplyAndSaveTheme(Theme theme)
  {
    ApplyTheme(theme);
    dataService.Set(DataKey.Theme, theme.Value);
  }
}

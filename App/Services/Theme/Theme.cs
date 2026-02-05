using System.Collections.Generic;
using System.Linq;

namespace App.Services.Theme;

public sealed class Theme(string value)
{
  public static readonly Theme Dark = new("Dark");
  public static readonly Theme Light = new("Light");
  public static readonly Theme System = new("System");

  private static readonly List<Theme> AllThemes = [Dark, Light, System];

  public string Value { get; } = value;

  public static Theme? FromValue(string? value)
  {
    return AllThemes.FirstOrDefault(t => t.Value == value);
  }
}

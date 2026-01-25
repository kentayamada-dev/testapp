namespace App.Services.Configuration;

public sealed class AppSettings
{
  public string LogFolder { get; set; } = "";
  public string AppcastUrl { get; set; } = "";
  public string SettingsFile { get; set; } = "";
  public string PublicKey { get; set; } = "";
}

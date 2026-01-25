namespace App.Services.Configuration;

public sealed class AppSettings
{
  public string SettingsFile { get; set; } = "";
  public string LogFolder { get; set; } = "";
  public string PublicKey { get; set; } = "";
  public string HomepageUrl { get; set; } = "";
  public string AppRepoUrl { get; set; } = "";
  public string AppcastUrl { get; set; } = "";
}

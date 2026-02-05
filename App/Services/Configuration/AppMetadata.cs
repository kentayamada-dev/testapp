namespace App.Services.Configuration;

public sealed class AppMetadata
{
  public required string AppName { get; init; }
  public required string AppVersion { get; init; }
  public required string CompanyName { get; init; }
}

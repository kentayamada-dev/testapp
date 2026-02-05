namespace App.Services.Configuration;

public sealed class AppFolders
{
  public required string BinFolder { get; init; }
  public required string DataFolder { get; init; }
  public required string LogFolder { get; init; }
}

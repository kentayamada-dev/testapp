using System;
using System.IO;
using System.Threading.Tasks;
using App.Services.Configuration;

namespace App.Services.Logger;

public static class LoggerService
{
  private static readonly string FolderPath = Path.Combine(
    ConfigurationService.AppDataFolder,
    ConfigurationService.AppSettings.SettingsFile
  );

  public static async Task Log(string message)
  {
    await WriteLogAsync(message);
  }

  public static async Task Log(Exception ex)
  {
    var fullMessage = $"{ex.Message}{Environment.NewLine}{ex}";
    await WriteLogAsync(fullMessage);
  }

  private static async Task WriteLogAsync(string message)
  {
    var logFilePath = Path.Combine(
      FolderPath,
      $"{DateTime.Now:yyyy-MM-dd}.txt"
    );
    var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";

    await File.AppendAllTextAsync(
      logFilePath,
      logEntry + Environment.NewLine
    );
  }
}

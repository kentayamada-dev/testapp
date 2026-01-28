using System;
using System.IO;
using System.Threading.Tasks;
using App.Services.Configuration;

namespace App.Services.Logger;

public static class LoggerService
{
  public static async Task Log(string message)
  {
    await WriteLogAsync(message);
  }

  public static async Task Log(Exception ex)
  {
    await WriteLogAsync(ex.ToString());
  }

  private static async Task WriteLogAsync(string message)
  {
    var logFilePath = Path.Combine(
      ConfigurationService.AppLogFolder,
      $"{DateTime.Now:yyyy-MM-dd}.txt"
    );
    var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";

    await File.AppendAllTextAsync(
      logFilePath,
      logEntry + Environment.NewLine
    );
  }
}

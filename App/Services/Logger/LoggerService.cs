using System;
using System.IO;
using System.Threading.Tasks;
using App.Services.Configuration;

namespace App.Services.Logger;

public class LoggerService
{
  private readonly string _folderPath;

  public LoggerService(string appDataFolder)
  {
    _folderPath = Path.Combine(appDataFolder, ConfigurationService.AppSettings.LogFolder);

    Directory.CreateDirectory(_folderPath);
  }

  public async Task Log(string message)
  {
    await WriteLogAsync(message);
  }

  public async Task Log(string message, Exception ex)
  {
    var fullMessage = $"{message}{Environment.NewLine}{ex}";
    await WriteLogAsync(fullMessage);
  }

  private async Task WriteLogAsync(string message)
  {
    var logFilePath = Path.Combine(_folderPath, $"{DateTime.Now:yyyy-MM-dd}.txt");
    var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";

    await File.AppendAllTextAsync(
      logFilePath,
      logEntry + Environment.NewLine
    );
  }
}

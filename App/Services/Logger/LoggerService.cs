using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace App.Services.Logger;

public sealed class LoggerService(string logFolder)
{
  private readonly SemaphoreSlim _semaphore = new(1, 1);

  public async Task Log(string message)
  {
    await WriteLogAsync(message);
  }

  public async Task Log(Exception ex)
  {
    await WriteLogAsync(ex.ToString());
  }

  private async Task WriteLogAsync(string message)
  {
    await _semaphore.WaitAsync();

    try
    {
      var logFilePath = Path.Combine(logFolder, $"{DateTime.Now:yyyy-MM-dd}.txt");
      var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";

      await File.AppendAllTextAsync(logFilePath, logEntry + Environment.NewLine);
    }
    finally
    {
      _semaphore.Release();
    }
  }
}

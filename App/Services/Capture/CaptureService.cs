using System;
using System.IO;
using System.Threading.Tasks;
using App.Services.Logger;
using FFMpegCore;

namespace App.Services.Capture;

public static class CaptureService
{
  public static async Task Capture(string url, string outputFolder)
  {
    var timestamp = DateTime.Now;
    var dateFolder = timestamp.ToString("yyyy-MM-dd");
    var hourFolder = timestamp.ToString("HH");
    var fileName = timestamp.ToString("mm-ss") + ".png";

    var fullPath = Path.Combine(outputFolder, dateFolder, hourFolder);
    Directory.CreateDirectory(fullPath);
    var outputPath = Path.Combine(fullPath, fileName);

    await FFMpegArguments
      .FromUrlInput(new System.Uri(url))
      .OutputToFile(outputPath, false, options => options
        .WithFrameOutputCount(1)
        .Seek(TimeSpan.FromSeconds(2)))
      .ProcessAsynchronously(true, new FFOptions { BinaryFolder = "./bin" });

    await LoggerService.Log($"Frame captured: {outputPath}");
  }
}

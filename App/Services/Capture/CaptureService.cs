using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using App.Services.Configuration;
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
      .FromUrlInput(new System.Uri(url), options => options
        .WithCustomArgument("-rtsp_transport tcp"))
      .OutputToFile(outputPath, false, options => options
        .WithFrameOutputCount(1)
        .Seek(TimeSpan.FromSeconds(2)))
      .ProcessAsynchronously(true, new FFOptions { BinaryFolder = ConfigurationService.AppBinFolder });

    await LoggerService.Log($"Frame captured: {outputPath}");
  }

  public static async Task CreateVideo(List<string> imageFiles, string videoFilePath, byte fps)
  {
    var tempListFile = Path.Combine(Path.GetTempPath(), "images_list.txt");
    var lines = imageFiles.Select(file => $"file '{Path.GetFullPath(file)}'\nduration {1 / fps}");

    await File.WriteAllTextAsync(tempListFile, string.Join("\n", lines));

    await FFMpegArguments.FromFileInput(new FileInfo(tempListFile), options => options.WithCustomArgument("-f concat").WithCustomArgument("-safe 0"))
      .OutputToFile(videoFilePath, true, options => options.WithFramerate(fps).ForcePixelFormat("yuv420p"))
      .ProcessAsynchronously(true, new FFOptions { BinaryFolder = ConfigurationService.AppBinFolder });

    await LoggerService.Log($"Video created: {videoFilePath}");
  }
}

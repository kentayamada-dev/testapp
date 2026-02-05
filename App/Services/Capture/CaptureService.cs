using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using App.Services.Logger;
using FFMpegCore;

namespace App.Services.Capture;

public sealed class CaptureService(LoggerService loggerService, string binaryFolder)
{
  public const string ImageExtension = ".png";
  private const string ImagesListFile = "images_list.txt";
  private const string VideoFile = "output.mp4";

  public async Task Capture(TimeSpan timeout, string url, string outputFolder, DateTime now)
  {
    var dateFolder = now.ToString("yyyy-MM-dd");
    var hourFolder = now.ToString("HH");
    var fileName = now.ToString("mm-ss") + ImageExtension;
    var fullPath = Path.Combine(outputFolder, dateFolder, hourFolder);
    var outputPath = Path.Combine(fullPath, fileName);

    Directory.CreateDirectory(fullPath);

    await FFMpegArguments
      .FromUrlInput(new System.Uri(url), options => options
        .WithCustomArgument("-rtsp_transport tcp")
        .WithCustomArgument($"-timeout {(uint)timeout.TotalMicroseconds}"))
      .OutputToFile(outputPath, false, options => options
        .WithFrameOutputCount(1)
        .Seek(TimeSpan.FromSeconds(2)))
      .ProcessAsynchronously(true, new FFOptions { BinaryFolder = binaryFolder });

    await loggerService.Log($"Frame captured: {outputPath}");
  }

  public async Task<string> CreateVideo(TimeSpan timeout, List<string> imageFiles, string inputFolder, byte fps)
  {
    var tempListFile = Path.Combine(inputFolder, ImagesListFile);
    var videoFilePath = Path.Combine(inputFolder, VideoFile);
    var lines = imageFiles.Select(file => $"file '{Path.GetFullPath(file)}'\nduration {1.0 / fps}");

    await File.WriteAllTextAsync(tempListFile, string.Join("\n", lines));

    await FFMpegArguments.FromFileInput(new FileInfo(tempListFile), options => options.WithCustomArgument("-f concat").WithCustomArgument("-safe 0"))
      .OutputToFile(videoFilePath, true,
        options => options.WithFramerate(fps).ForcePixelFormat("yuv420p").WithCustomArgument($"-timeout {(uint)timeout.TotalMicroseconds}"))
      .ProcessAsynchronously(true, new FFOptions { BinaryFolder = binaryFolder });

    await loggerService.Log($"Video created: {videoFilePath}");

    return videoFilePath;
  }
}

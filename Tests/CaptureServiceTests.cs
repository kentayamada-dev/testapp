using App.Services.Capture;
using App.Services.Logger;
using Microsoft.Extensions.Configuration;
using Tests.Extensions;

namespace Tests;

public class CaptureServiceTests
{
  private const string ImageExt = ".png";
  private readonly IConfiguration _config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
  private readonly CaptureService _service;

  public CaptureServiceTests()
  {
    var logger = new LoggerService(TempDir);
    _service = new CaptureService(logger, "ffmpeg");
  }

  private string TempDir => _config.Get("CaptureService:TempDir");

  [Fact]
  public async Task Capture()
  {
    var now = new DateTime(2026, 2, 3, 19, 30, 45);

    for (var i = 0; i < 10; i++)
    {
      var timestamp = now.AddSeconds(i * 5);
      await _service.Capture(TimeSpan.FromSeconds(5), _config.Get("CaptureService:RtspUrl"), TempDir, timestamp);
    }

    var expected = Path.Combine(TempDir, "2026-02-03", "19");
    Assert.True(Directory.Exists(expected));

    var imageCount = Directory.GetFiles(expected, $"*{ImageExt}", SearchOption.AllDirectories).Length;

    Assert.Equal(10, imageCount);
  }

  [Fact]
  public async Task CreateVideo()
  {
    var imageFiles = Directory.GetFiles(TempDir, $"*{ImageExt}", SearchOption.AllDirectories).ToList();

    await _service.CreateVideo(TimeSpan.FromHours(1), imageFiles, TempDir, 1);

    Assert.True(File.Exists(_config.Get("GoogleService:VideoPath")));
  }
}

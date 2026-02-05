using App.Services.Google;
using Microsoft.Extensions.Configuration;
using Tests.Extensions;
using Xunit.Abstractions;

namespace Tests;

public class GoogleServiceTests(ITestOutputHelper testOutputHelper) : IAsyncLifetime
{
  private readonly IConfiguration _config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
  private readonly GoogleService _service = new();

  public async Task InitializeAsync()
  {
    await _service.Initialize(_config.Get("GoogleService:CredentialsPath"));
  }

  public Task DisposeAsync()
  {
    return Task.CompletedTask;
  }

  [Fact]
  public async Task UploadVideo()
  {
    var videoId = await _service.UploadVideo("Test Video Title", _config.Get("GoogleService:VideoTitle"), OnProgress);

    Assert.NotEmpty(videoId);
    return;

    void OnProgress(int percentage)
    {
      testOutputHelper.WriteLine(percentage.ToString());
    }
  }

  [Fact]
  public async Task AppendDataToSheet()
  {
    var values = new List<IList<object>>
    {
      new List<object> { "Name", "Age", "City" },
      new List<object> { "John", 30, "New York" },
      new List<object> { "Jane", 28, "Los Angeles" }
    };

    await _service.AppendDataToSheet("Sheet1", _config.Get("GoogleService:SpreadsheetName"), values);

    Assert.True(true);
  }
}

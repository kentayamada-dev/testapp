using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using App.Services.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace App.Services.Google;

public sealed class GoogleService
{
  private UserCredential? _credential;
  private SheetsService? _sheetsService;
  private YouTubeService? _youtubeService;

  public async Task Initialize(string credentialsPath)
  {
    await using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
    {
      _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
        (await GoogleClientSecrets.FromStreamAsync(stream)).Secrets,
        [
          YouTubeService.Scope.YoutubeUpload,
          SheetsService.Scope.Spreadsheets
        ],
        "user",
        CancellationToken.None,
        new FileDataStore(ConfigurationService.AppFolders.DataFolder)
      );
    }

    var initializer = new BaseClientService.Initializer
    {
      HttpClientInitializer = _credential,
      ApplicationName = ConfigurationService.AppMetadata.AppName
    };

    _youtubeService = new YouTubeService(initializer);
    _sheetsService = new SheetsService(initializer);
  }

  public async Task<string> UploadVideo(string filePath, string title, Action<int>? onProgress)
  {
    if (_youtubeService == null) throw new InvalidOperationException("YouTube Service is not initialized.");

    var videoId = "";

    var video = new Video
    {
      Snippet = new VideoSnippet
      {
        Title = title
      },
      Status = new VideoStatus { PrivacyStatus = "private" }
    };

    await using var fileStream = new FileStream(filePath, FileMode.Open);

    var totalBytes = fileStream.Length;

    var insertRequest = _youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");

    if (onProgress != null)
      insertRequest.ProgressChanged += progress =>
      {
        var percentage = (int)(progress.BytesSent / (double)totalBytes * 100);

        onProgress(percentage);
      };

    insertRequest.ResponseReceived += uploadedVideo => { videoId = uploadedVideo.Id; };

    await insertRequest.UploadAsync();

    return videoId;
  }

  public async Task AppendDataToSheet(string spreadsheetId, string sheetName, IList<IList<object>> values)
  {
    if (_sheetsService == null) throw new InvalidOperationException("Sheets Service is not initialized.");

    var request = _sheetsService.Spreadsheets.Values.Append(new ValueRange { Values = values }, spreadsheetId, $"{sheetName}!A1");

    request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;

    await request.ExecuteAsync();
  }
}

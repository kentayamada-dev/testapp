using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using App.Models;
using App.Services.Configuration;

namespace App.Services.Data;

public class UserPreferencesModel
{
  public string? Theme { get; set; }

  public string? Culture { get; set; }

  public CaptureFormModel? CaptureForm { get; set; }

  public UploadFormModel? UploadForm { get; set; }
}

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
  DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(UserPreferencesModel))]
public partial class DataPersistJsonContext : JsonSerializerContext;

public static class DataPersistService
{
  private static readonly string FilePath = Path.Combine(
    ConfigurationService.AppDataFolder,
    ConfigurationService.AppSettings.SettingsFile
  );

  private static UserPreferencesModel? _cache;

  public static void Initialize()
  {
    if (!File.Exists(FilePath))
    {
      _cache = new UserPreferencesModel();
      var json = JsonSerializer.Serialize(
        _cache,
        DataPersistJsonContext.Default.UserPreferencesModel
      );
      File.WriteAllText(FilePath, json);
    }
    else
    {
      var json = File.ReadAllText(FilePath);
      _cache = JsonSerializer.Deserialize(
        json,
        DataPersistJsonContext.Default.UserPreferencesModel
      ) ?? new UserPreferencesModel();
    }
  }

  public static UserPreferencesModel Get()
  {
    return _cache ?? throw new InvalidOperationException(
      "Service not initialized. Call Initialize() first"
    );
  }

  public static async Task Save(UserPreferencesModel settings)
  {
    var json = JsonSerializer.Serialize(
      settings,
      DataPersistJsonContext.Default.UserPreferencesModel
    );
    await File.WriteAllTextAsync(FilePath, json);
    _cache = settings;
  }

  public static async Task Update(
    Action<UserPreferencesModel> updateAction
  )
  {
    var cache = _cache ?? throw new InvalidOperationException(
      "Service not initialized. Call Initialize() first"
    );
    updateAction(cache);
    await Save(cache);
  }
}

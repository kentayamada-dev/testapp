using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using App.Services.Configuration;

namespace App.Services.Data;

public enum DataKey
{
  Culture,
  Theme
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<string, string>))]
public sealed partial class DataSerializerContext
  : JsonSerializerContext;

public sealed class DataPersistService
{
  private readonly Dictionary<string, string> _data;
  private readonly string _settingsFile;

  public DataPersistService(string appDataFolder)
  {
    _settingsFile = Path.Combine(
      appDataFolder,
      ConfigurationService.AppSettings.SettingsFile);
    _data = Load();
  }

  public void Set(DataKey key, string value)
  {
    _data[key.ToString()] = value;
    Save();
  }

  public string? Get(DataKey key)
  {
    return _data.GetValueOrDefault(key.ToString());
  }

  public void Remove(string key)
  {
    if (_data.Remove(key)) Save();
  }

  private Dictionary<string, string> Load()
  {
    if (!File.Exists(_settingsFile))
      return new Dictionary<string, string>();

    var json = File.ReadAllText(_settingsFile);
    return JsonSerializer.Deserialize<Dictionary<string, string>>(
      json,
      DataSerializerContext.Default
        .DictionaryStringString
    ) ?? new Dictionary<string, string>();
  }

  private void Save()
  {
    var json = JsonSerializer.Serialize(
      _data,
      DataSerializerContext.Default.DictionaryStringString
    );
    File.WriteAllText(_settingsFile, json);
  }
}

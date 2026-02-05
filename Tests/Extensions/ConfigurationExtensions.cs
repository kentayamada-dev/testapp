using Microsoft.Extensions.Configuration;

namespace Tests.Extensions;

public static class ConfigurationExtensions
{
  public static string Get(this IConfiguration config, string key)
  {
    return config[key] ?? throw new InvalidOperationException($"Missing required config: {key}");
  }
}

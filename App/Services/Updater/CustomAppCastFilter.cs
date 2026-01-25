using System.Collections.Generic;
using System.Linq;
using NetSparkleUpdater;
using NetSparkleUpdater.AppCastHandlers;
using NetSparkleUpdater.Interfaces;

namespace App.Services.Updater;

public sealed class CustomAppCastFilter : IAppCastFilter
{
  public string OperatingSystem { get; init; } = "";
  public string AppVersion { get; init; } = "";

  public IEnumerable<AppCastItem> GetFilteredAppCastItems(SemVerLike installed, IEnumerable<AppCastItem> items)
  {
    return items.Where(item =>
    {
      if (SemVerLike.Parse(item.Version).CompareTo(SemVerLike.Parse(AppVersion)) <= 0) return false;
      return item.OperatingSystem == OperatingSystem;
    });
  }
}

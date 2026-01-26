using System.Threading.Tasks;
using Avalonia.Controls;

namespace App.Services.Uri;

public static class UriService
{
  public static async Task OpenUri(string? url, Window owner)
  {
    var topLevel = TopLevel.GetTopLevel(owner);

    if (topLevel == null || url == null) return;

    await topLevel.Launcher.LaunchUriAsync(new System.Uri(url));
  }
}

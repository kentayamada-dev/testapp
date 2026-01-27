using System.Globalization;
using System.Linq;
using App.Assets.Culture;

namespace App.Services.Culture;

public static class CultureService
{
  public static Culture GetCulture(string? culture)
  {
    return Culture.GetAll().FirstOrDefault(availableCulture => availableCulture.Code == culture) ??
           Culture.En;
  }

  public static void ApplyCulture(Culture culture)
  {
    var cultureInfo = new CultureInfo(culture.Code);
    Resources.Culture = cultureInfo;
    CultureInfo.CurrentCulture = cultureInfo;
    CultureInfo.CurrentUICulture = cultureInfo;
  }
}

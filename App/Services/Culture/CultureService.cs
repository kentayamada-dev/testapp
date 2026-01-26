using System.Globalization;
using System.Linq;
using App.Assets.Culture;
using App.Services.Application;
using App.Services.Data;

namespace App.Services.Culture;

public sealed class CultureService(DataPersistService dataService)
{
  public static Culture GetCulture(string? culture)
  {
    return Culture.GetAll().FirstOrDefault(availableCulture => availableCulture.Code == culture) ?? Culture.En;
  }

  public static void ApplyCulture(Culture culture)
  {
    if (CultureInfo.CurrentCulture.Name == culture.Code) return;

    var cultureInfo = new CultureInfo(culture.Code);
    Resources.Culture = cultureInfo;
    CultureInfo.CurrentCulture = cultureInfo;
    CultureInfo.CurrentUICulture = cultureInfo;
  }

  public void ApplyAndSaveCulture(Culture culture)
  {
    ApplyCulture(culture);
    dataService.Set(DataKey.Culture, culture.Code);
    ApplicationService.RestartApplication();
  }
}

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

  public void SetCulture(Culture culture, bool save = true)
  {
    if (CultureInfo.CurrentCulture.Name == culture.Code) return;

    var cultureInfo = new CultureInfo(culture.Code);
    Resources.Culture = cultureInfo;
    CultureInfo.CurrentCulture = cultureInfo;
    CultureInfo.CurrentUICulture = cultureInfo;

    if (!save) return;

    dataService.Set(DataKey.Culture, culture.Code);
    ApplicationService.RestartApplication();
  }
}

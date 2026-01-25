using System.IO;
using App.Services.Culture;
using App.Services.Data;
using App.Services.Logger;
using App.Services.Theme;
using App.Services.Updater;
using App.Views;
using Microsoft.Extensions.DependencyInjection;

namespace App.Services;

public static class ServiceCollectionExtensions
{
  public static void AddAppServices(this IServiceCollection services,
    string appDataFolder)
  {
    Directory.CreateDirectory(appDataFolder);

    services.AddSingleton<MainWindow>();
    services.AddTransient<DialogWindow>();
    services.AddSingleton<CultureService>();
    services.AddSingleton<ThemeService>();
    services.AddSingleton<UpdaterService>();
    services.AddSingleton(new DataPersistService(appDataFolder));
    services.AddSingleton(new LoggerService(appDataFolder));
  }
}

using App.Services.Capture;
using App.Services.Configuration;
using App.Services.Google;
using App.Services.Logger;
using App.Services.Updater;
using App.ViewModels;
using App.Views;
using Microsoft.Extensions.DependencyInjection;

namespace App.Services;

public static class ServiceCollectionExtensions
{
  public static void AddAppServices(this IServiceCollection services)
  {
    services.AddTransient<MessageDialog>();
    services.AddTransient<ConfirmDialog>();
    services.AddTransient<ProgressDialog>();

    services.AddSingleton<MainViewModel>();
    services.AddSingleton<CaptureFormViewModel>();
    services.AddSingleton<UploadFormViewModel>();
    services.AddSingleton<TextInputWithButtonViewModel>();
    services.AddSingleton<SettingsViewModel>();
    services.AddSingleton<UpdaterService>();
    services.AddSingleton<GoogleService>();
    services.AddSingleton(new LoggerService(ConfigurationService.AppFolders.LogFolder));
    services.AddSingleton(provider => new CaptureService(provider.GetRequiredService<LoggerService>(), ConfigurationService.AppFolders.BinFolder));
  }
}

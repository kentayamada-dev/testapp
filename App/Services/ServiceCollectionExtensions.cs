using App.Services.Updater;
using App.ViewModels;
using App.Views;
using Microsoft.Extensions.DependencyInjection;

namespace App.Services;

public static class ServiceCollectionExtensions
{
  public static void AddAppServices(this IServiceCollection services)
  {
    services.AddSingleton<MainViewModel>();
    services.AddSingleton<CaptureFormViewModel>();
    services.AddSingleton<UploadFormViewModel>();
    services.AddSingleton<TextInputWithButtonViewModel>();
    services.AddSingleton<SettingsViewModel>();
    services.AddTransient<MessageDialog>();
    services.AddTransient<ConfirmDialog>();
    services.AddTransient<ProgressDialog>();
    services.AddSingleton<UpdaterService>();
  }
}

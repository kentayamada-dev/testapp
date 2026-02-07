using App.Services.Configuration;
using NetSparkleUpdater.Interfaces;

namespace App.Services.Updater;

public class CustomAssemblyAccessor:IAssemblyAccessor
{
  public string AssemblyVersion => ConfigurationService.AppMetadata.AppVersion;

  public string AssemblyTitle => ConfigurationService.AppMetadata.AppName;

  public string AssemblyCompany => ConfigurationService.AppMetadata.CompanyName;

  public string AssemblyDescription => ConfigurationService.AppMetadata.Description;

  public string AssemblyCopyright => ConfigurationService.AppMetadata.Copyright;

  public string AssemblyProduct => ConfigurationService.AppMetadata.AssemblyName;
}

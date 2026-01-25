using System;
using System.Linq;
using System.Threading.Tasks;
using App.Services.Application;
using App.Services.Configuration;
using App.Services.Logger;
using App.Views;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.SignatureVerifiers;

namespace App.Services.Updater;

public class UpdaterService(
  LoggerService loggerService,
  IServiceProvider serviceProvider)
{
  private readonly SparkleUpdater _sparkleUpdater = new CustomSparkleUpdater(
    ConfigurationService.AppSettings.AppcastUrl,
    new Ed25519Checker(
      SecurityMode.Strict,
      ConfigurationService.AppSettings.PublicKey
    ),
    ConfigurationService.AppMetadata.AppVersion,
    ConfigurationService.AppMetadata.AppName
  );

  private string? _downloadPath;
  private int _lastReportedProgress = -1;
  private ProgressWindow? _progressWindow;

  public async Task CheckForUpdate(Window owner)
  {
    try
    {
      var updateInfo =
        await _sparkleUpdater.CheckForUpdatesQuietly();
      await HandleUpdateCheckResult(updateInfo, owner);
    }
    catch (Exception ex)
    {
      await ShowMessageDialog(
        owner,
        "An error occurred while checking for updates."
      );
      await loggerService.Log(
        "An error occurred while checking for updates.",
        ex
      );
    }
  }

  private async Task HandleUpdateCheckResult(
    UpdateInfo updateInfo,
    Window owner
  )
  {
    switch (updateInfo.Status)
    {
      case UpdateStatus.UpdateAvailable:
        var confirmDialog =
          serviceProvider.GetRequiredService<DialogWindow>();
        confirmDialog.SetMessage(
          "New update available! Update now?"
        );
        var result =
          await confirmDialog.ShowDialog<DialogResult>(owner);

        if (result != DialogResult.Ok) return;

        await HandleInstallUpdate(owner, updateInfo);

        return;
      case UpdateStatus.UpdateNotAvailable:
        await ShowMessageDialog(
          owner,
          "There's no update available :("
        );
        break;
      case UpdateStatus.UserSkipped:
      case UpdateStatus.CouldNotDetermine:
      default:
        await ShowMessageDialog(
          owner,
          "Unable to determine update status."
        );
        break;
    }
  }

  private async Task HandleInstallUpdate(
    Window owner,
    UpdateInfo updateInfo
  )
  {
    _progressWindow = serviceProvider.GetRequiredService<ProgressWindow>();
    _progressWindow.Show(owner);
    try
    {
      var update = updateInfo.Updates.First();
      AttachUpdateEventHandlers();
      await _sparkleUpdater.InitAndBeginDownload(update);
      await _sparkleUpdater.InstallUpdate(update, _downloadPath);
    }
    catch (Exception ex)
    {
      await ShowMessageDialog(
        owner,
        "An error occurred while installing the update."
      );
      await loggerService.Log(
        "An error occurred while installing the update.",
        ex
      );
    }
  }

  private async Task ShowMessageDialog(
    Window owner,
    string message
  )
  {
    var dialog =
      serviceProvider.GetRequiredService<DialogWindow>();
    dialog.SetMessage(message);
    dialog.SetShowCancelButton(false);
    await dialog.ShowDialog<DialogResult>(owner);
  }

  private void OnDownloadFinished(AppCastItem item, string path)
  {
    _downloadPath = path;
  }

  private void OnDownloadMadeProgress(object sender, AppCastItem item, ItemDownloadProgressEventArgs e)
  {
    if (e.ProgressPercentage == _lastReportedProgress) return;

    _lastReportedProgress = e.ProgressPercentage;
    _progressWindow?.SetProgress(_lastReportedProgress);
  }

  private void AttachUpdateEventHandlers()
  {
    _sparkleUpdater.DownloadMadeProgress += OnDownloadMadeProgress;
    _sparkleUpdater.DownloadFinished += OnDownloadFinished;
    _sparkleUpdater.CloseApplication +=
      ApplicationService.CloseApplication;
  }
}

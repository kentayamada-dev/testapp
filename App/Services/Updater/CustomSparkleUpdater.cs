using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Interfaces;

namespace App.Services.Updater;

public sealed class CustomSparkleUpdater : SparkleUpdater
{
  private readonly string _appName;
  private readonly bool _isMac;
  private readonly bool _isWindows;

  public CustomSparkleUpdater(string appcastUrl, ISignatureVerifier signatureVerifier, string appVersion, string appName) : base(appcastUrl,
    signatureVerifier, null)
  {
    _appName = appName;
    _isWindows = OperatingSystem.IsWindows();
    _isMac = OperatingSystem.IsMacOS();

    AppCastHelper.AppCastFilter = new CustomAppCastFilter
    {
      OperatingSystem = GetCurrentOperatingSystem(),
      AppVersion = appVersion
    };

    CheckServerFileName = false;

    UserInteractionMode = UserInteractionMode.DownloadAndInstall;
  }

  protected override string GetInstallerCommand(string downloadFilePath)
  {
    var installerExt = Path.GetExtension(downloadFilePath);

    if (_isMac && DoExtensionsMatch(installerExt, ".pkg")) return $"sudo installer -pkg \"{downloadFilePath}\" -target /";

    if (_isWindows) return $"\"{downloadFilePath}\" /VERYSILENT";

    return base.GetInstallerCommand(downloadFilePath);
  }

  protected override async Task RunDownloadedInstaller(string downloadFilePath)
  {
    var executableName = RestartExecutableName;
    var workingDir = RestartExecutablePath;
    var extension = _isWindows ? ".cmd" : ".sh";
    var batchFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + extension);
    var installerCmd = GetInstallerCommand(downloadFilePath);
    var processId = Environment.ProcessId.ToString();

    await using (var stream = new FileStream(batchFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, true))
    await using (var write = new StreamWriter(stream, new UTF8Encoding(false)))
    {
      if (_isWindows)
      {
        var relaunchAfterUpdate = $@"
                    cd ""{workingDir}""
                    ""{executableName}""";

        var output = $@"
                        @echo off
                        chcp 65001 > nul
                        set /A counter=0
                        setlocal ENABLEDELAYEDEXPANSION
                        :loop
                        set /A counter=!counter!+1
                        if !counter! == 90 (
                            exit /b 1
                        )
                        tasklist | findstr ""\<{processId}\>"" > nul
                        if not errorlevel 1 (
                            timeout /t 1 > nul
                            goto :loop
                        )
                        :install
                        {installerCmd}
                        :afterinstall
                        {relaunchAfterUpdate.Trim()}
                        endlocal";
        await write.WriteAsync(output);
        write.Close();
      }
      else
      {
        var waitForFinish = $@"
                        COUNTER=0;
                        while ps -p {processId} > /dev/null;
                            do sleep 1;
                            COUNTER=$((++COUNTER));
                            if [ $COUNTER -eq 90 ]
                            then
                                exit -1;
                            fi;
                        done;
                    ";

        if (_isMac)
        {
          var relaunchCmd = $"open -a \"{_appName}\"";

          var output = $@"
                {waitForFinish}
                {installerCmd}
                {relaunchCmd}";
          await write.WriteAsync(output.Replace("\r\n", "\n"));
        }

        write.Close();
        Exec($"chmod +x {batchFilePath}");
      }
    }

    if (_isWindows)
    {
      _installerProcess = new Process
      {
        StartInfo =
        {
          FileName = batchFilePath,
          WindowStyle = ProcessWindowStyle.Hidden,
          UseShellExecute = false,
          CreateNoWindow = true
        }
      };
      _installerProcess.Start();
    }
    else
    {
      if (_isMac)
        ExecMacSudo(batchFilePath);
      else
        Exec(batchFilePath, false);
    }

    await QuitApplication();
  }

  private void ExecMacSudo(string cmd)
  {
    var escapedArgs = cmd.Replace("\"", "\\\"");
    _installerProcess = new Process
    {
      StartInfo = new ProcessStartInfo
      {
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true,
        WindowStyle = ProcessWindowStyle.Hidden,
        FileName = "osascript",
        Arguments = $"-e \"do shell script \\\"{escapedArgs}\\\" with administrator privileges\""
      }
    };
    _installerProcess.Start();
  }

  private static string GetCurrentOperatingSystem()
  {
    var os = OperatingSystem.IsWindows()
      ? "windows"
      : OperatingSystem.IsMacOS()
        ? "macos"
        : throw new PlatformNotSupportedException(
          "Unsupported operating system");

    var arch = RuntimeInformation.ProcessArchitecture switch
    {
      Architecture.X64 => "x64",
      Architecture.Arm64 => "arm64",
      _ => throw new PlatformNotSupportedException(
        "Unsupported processor architecture")
    };

    return $"{os}-{arch}";
  }
}

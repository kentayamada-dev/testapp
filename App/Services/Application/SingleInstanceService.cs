using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using App.Services.Configuration;
using Avalonia.Controls;
using Avalonia.Threading;

namespace App.Services.Application;

public class SingleInstanceService : IDisposable
{
  private readonly string _activateEventName = $"{ConfigurationService.AppMetadata.AppName} Event";
  private readonly string _instanceMutexName = $"{ConfigurationService.AppMetadata.AppName} Mutex";

  private EventWaitHandle? _activateEvent;
  private CancellationTokenSource? _activateListenerCts;
  private Mutex? _instanceMutex;
  private Window? _mainWindow;

  public bool IsNewInstance { get; private set; }

  public void Dispose()
  {
    _activateListenerCts?.Cancel();
    _activateListenerCts?.Dispose();
    _activateEvent?.Dispose();

    if (_instanceMutex != null)
    {
      if (IsNewInstance) _instanceMutex.ReleaseMutex();

      _instanceMutex.Dispose();
    }

    GC.SuppressFinalize(this);
  }

  [SupportedOSPlatform("windows")]
  public void CheckSingleInstance()
  {
    _instanceMutex = new Mutex(true, _instanceMutexName, out var isNewInstance);
    IsNewInstance = isNewInstance;

    if (!isNewInstance)
    {
      using var existing = EventWaitHandle.OpenExisting(_activateEventName);
      existing.Set();
      return;
    }

    _activateEvent = new EventWaitHandle(false, EventResetMode.AutoReset, _activateEventName);
  }

  public void StartActivateListener(Window mainWindow)
  {
    if (!IsNewInstance) return;

    if (_activateEvent == null)
      throw new InvalidOperationException(
        "Activate event is not initialized. Call CheckSingleInstance() first."
      );

    _mainWindow = mainWindow;
    _activateListenerCts = new CancellationTokenSource();

    var token = _activateListenerCts.Token;
    var handles = new[] { _activateEvent, token.WaitHandle };

    Task.Run(
      () =>
      {
        while (WaitHandle.WaitAny(handles) == 0) Dispatcher.UIThread.Post(BringWindowToFront);
      }, token);
  }

  public void BringWindowToFront()
  {
    if (_mainWindow == null) return;

    if (_mainWindow.WindowState == WindowState.Minimized) _mainWindow.WindowState = WindowState.Normal;

    _mainWindow.Show();
    _mainWindow.Activate();

    _mainWindow.Topmost = true;
    _mainWindow.Topmost = false;
  }
}

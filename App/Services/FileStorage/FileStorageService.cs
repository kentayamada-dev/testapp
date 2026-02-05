using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace App.Services.FileStorage;

public static class FileStorageService
{
  public static async Task<bool> IsValidFolderPath(Window? window, string? folderPath)
  {
    if (window == null || folderPath == null) return false;

    var folder = await window.StorageProvider.TryGetFolderFromPathAsync(folderPath);

    return folder != null;
  }

  public static async Task<string?> GetSelectedFolderPath(Window? window)
  {
    if (window == null) return null;

    var folders = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions());

    return folders.Count > 0 ? folders[0].Path.LocalPath : null;
  }

  public static async Task<bool> IsValidFilePath(Window? window, string? filePath)
  {
    if (window == null || filePath == null) return false;

    var file = await window.StorageProvider.TryGetFileFromPathAsync(filePath);

    return file != null;
  }

  public static async Task<string?> GetSelectedFilePath(Window? window, IReadOnlyList<FilePickerFileType>? fileTypeFilter = null)
  {
    if (window == null) return null;

    var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { FileTypeFilter = fileTypeFilter });

    return files.Count > 0 ? files[0].Path.LocalPath : null;
  }
}

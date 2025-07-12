using System.IO;

namespace PCL.Core.Model.Files;

/// <summary>
/// Process a loaded <see cref="FileItem"/>.
/// </summary>
/// <param name="item">the item to process</param>
/// <param name="path">the real path of the file, or <c>null</c> if the file is not found and fail to transfer</param>
/// <returns>process result</returns>
public delegate object? FileProcess(FileItem item, string? path);

public static class FileProcesses
{
    public static readonly FileProcess ReadText = ((_, path) =>
    {
        if (path == null) return null;
        var fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
        var sr = new StreamReader(fs);
        return sr.ReadToEnd();
    });
}

using System;

namespace PCL.Core.Model.Files;

/// <summary>
/// Transfer a <see cref="FileItem"/> from predefined <see cref="FileItem.Sources"/>
/// to <see cref="FileItem.TargetPath"/>, return the real path by <paramref name="resultCallback"/>,
/// or <c>null</c> if transfer has failed
/// </summary>
public delegate void FileTransfer(FileItem item, Action<string?> resultCallback);

/// <summary>
/// Mark a transfer as failed. The file service will try next transfer automatically.
/// </summary>
/// <param name="reason">failed reason</param>
/// <param name="item">the failed file item</param>
/// <param name="innerException">the exception causing the fail</param>
public class TransferFailedException(string reason, FileItem item, Exception? innerException = null)
    : Exception($"{reason}: {item}", innerException)
{
    public FileItem FileItem { get; } = item;
    public string Reason { get; } = reason;
}

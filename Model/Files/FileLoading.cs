using System;

namespace PCL.Core.Model.Files;

/// <summary>
/// Match a <see cref="FileItem"/>.
/// </summary>
/// <param name="item">the item to match</param>
/// <returns>match result</returns>
public delegate bool FileMatch(FileItem item);

/// <summary>
/// Transfer a <see cref="FileItem"/> from predefined <see cref="FileItem.Sources"/>
/// to <see cref="FileItem.TargetPath"/>, return the real path by <paramref name="resultCallback"/>,
/// or <c>null</c> if transfer has failed
/// </summary>
public delegate void FileTransfer(FileItem item, Action<string?> resultCallback);

/// <summary>
/// Process a loaded <see cref="FileItem"/>.
/// </summary>
/// <param name="item">the item to process</param>
/// <param name="path">the real path of the file, or <c>null</c> if the file is not found and fail to transfer</param>
/// <returns>process result</returns>
public delegate object? FileProcess(FileItem item, string? path);

public class FileMatchPair<TValue>(FileMatch match, TValue value)
{
    public bool Match(FileItem item) => match(item);
    public TValue Value => value;
}

public static class FileMatchExtension
{
    public static FileMatchPair<TValue> Pair<TValue>(this FileMatch match, TValue value) => new(match, value);
}

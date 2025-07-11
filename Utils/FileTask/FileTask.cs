using System;
using System.Collections.Generic;
using System.Linq;
using PCL.Core.Model.Files;

namespace PCL.Core.Utils.FileTask;

/// <summary>
/// Default implementation of <see cref="IFileTask"/>.
/// </summary>
/// <param name="items">See <see cref="Items"/></param>
/// <param name="ignoreResult">See <see cref="IgnoreResult"/></param>
public class FileTask(IEnumerable<FileItem> items, bool ignoreResult = false) : IFileTask
{
    /// <summary>
    /// <see cref="FileItem"/> instances.
    /// </summary>
    public IEnumerable<FileItem> Items { get; } = items;

    /// <summary>
    /// Ignore <see cref="ProcessFinished"/> event and drop the result.
    /// </summary>
    public bool IgnoreResult { get; set; } = ignoreResult;

    /// <summary>
    /// Create an empty task. (Anyone really use this?)
    /// </summary>
    /// <param name="ignoreResult">See <see cref="IgnoreResult"/></param>
    public FileTask(bool ignoreResult = false) : this([], ignoreResult) { }
    
    /// <summary>
    /// Create a task with file items
    /// </summary>
    /// <param name="items">file items</param>
    public FileTask(params FileItem[] items) : this(items.AsEnumerable()) { }
    
    /// <summary>
    /// Event invoked after a process finished
    /// </summary>
    public event Action<FileItem, object?>? ProcessFinished;
    
    /// <summary>
    /// Event invoked after the task finished
    /// </summary>
    public event Action? TaskFinished;

    #region Implementation

    public virtual FileTransfer? GetTransfer(FileItem item) => null;
    
    public virtual FileProcess? GetProcess(FileItem item) => null;
    
    public virtual bool OnProcessFinished(FileItem item, object? result)
    {
        if (IgnoreResult) return true;
        if (ProcessFinished == null) return false;
        ProcessFinished.Invoke(item, result);
        return true;
    }

    public virtual void OnTaskFinished()
    {
        TaskFinished?.Invoke();
    }

    #endregion
}

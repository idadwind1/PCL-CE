using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using PCL.Core.Helper;
using PCL.Core.LifecycleManagement;
using PCL.Core.Model.Files;
using PCL.Core.Utils.Threading;
using Special = System.Environment.SpecialFolder;

namespace PCL.Core.Service;

/// <summary>
/// Global file management service.
/// </summary>
[LifecycleService(LifecycleState.Loading, Priority = 1919820)]
public sealed class FileService : GeneralService
{

    #region Paths

    /// <summary>
    /// The default directory used for relative path combining.
    /// </summary>
    public static string DefaultDirectory => NativeInterop.ExecutableDirectory;

    private static string _dataPath = @"PCL\CE";
    private static string _sharedDataPath = @"PCL\CE\_Data";
    private static string _localDataPath = @"PCL\CE\_Local";
    private static string _tempPath = @"PCL\CE\_Temp";
    
    /// <summary>
    /// Per-instance data directory.
    /// </summary>
    public static string DataPath { get => _dataPath; set => _dataPath = value; }

    /// <summary>
    /// Shared synchronized data directory.
    /// </summary>
    public static string SharedDataPath { get => _sharedDataPath; set => _sharedDataPath = value; }
    
    /// <summary>
    /// Shared local data directory, used to put some large files that can be released or downloaded back anytime.
    /// </summary>
    public static string LocalDataPath { get => _localDataPath; set => _localDataPath = value; }
    
    /// <summary>
    /// Temporary files directory (can be deleted anytime, except when the program is running).
    /// </summary>
    public static string TempPath { get => _tempPath; set => _tempPath = value; }

    /// <summary>
    /// Get path string relative to a special folder.
    /// </summary>
    /// <param name="folder">the special folder</param>
    /// <param name="relative">the relative path</param>
    /// <returns>the path string relative to the special folder</returns>
    public static string GetSpecialPath(Special folder, string relative)
    {
        var folderPath = Environment.GetFolderPath(folder);
        return Path.Combine(folderPath, relative);
    }

    #endregion

    #region Lifecycle

    private static LifecycleContext? _context;
    private static LifecycleContext Context => _context!;

    private FileService() : base("file", "文件管理") { _context = ServiceContext; }
    
    public override void Start()
    {
        // correct paths
        const string name = "PCLCE";
        Context.Debug($"正在替换存储路径，目录名 {name}");
        DataPath = Path.Combine(DefaultDirectory, "PCL\\CE");
        SharedDataPath = GetSpecialPath(Special.ApplicationData, name);
        LocalDataPath = GetSpecialPath(Special.LocalApplicationData, name);
        TempPath = GetSpecialPath(Special.LocalApplicationData, $"Temp\\{name}");
#if DEBUG
        // read environment variables
        NativeInterop.ReadEnvironmentVariable("PCL_PATH", ref _dataPath);
        NativeInterop.ReadEnvironmentVariable("PCL_PATH_SHARED", ref _sharedDataPath);
        NativeInterop.ReadEnvironmentVariable("PCL_PATH_LOCAL", ref _localDataPath);
        NativeInterop.ReadEnvironmentVariable("PCL_PATH_TEMP", ref _tempPath);
#endif
        // start load thread
        NativeInterop.RunInNewThread(_FileLoadCallback, "Daemon/FileLoading");
    }

    public override void Stop()
    {
        Context.Debug("尝试停止文件处理工作");
        _running = false;
    }

    #endregion

    #region Process

    private static readonly List<FileMatchPair<FileTransfer>> _DefaultTransfers = [];
    private static readonly List<FileMatchPair<FileProcess>> _DefaultProcesses = [];

    /// <summary>
    /// Register a transfer implementation with a match.
    /// </summary>
    public static void RegisterDefaultTransfer(FileMatch match, FileTransfer transfer)
        => _DefaultTransfers.Add(match.Pair(transfer));
    
    /// <summary>
    /// Register a process implementation with a match.
    /// </summary>
    public static void RegisterDefaultProcess(FileMatch match, FileProcess process)
        => _DefaultProcesses.Add(match.Pair(process));

    private static FileTransfer? _MatchDefaultTransfer(FileItem item)
    {
        try { return _DefaultTransfers.First(pair => pair.Match(item)).Value; }
        catch { return null; }
    }
    
    private static FileProcess? _MatchDefaultProcess(FileItem item)
    {
        try { return _DefaultProcesses.First(pair => pair.Match(item)).Value; }
        catch { return null; }
    }

    private static readonly ConcurrentQueue<IFileTask> _PendingTasks = [
    ];
    
    private static readonly AutoResetEvent _ContinueEvent = new(false);
    private static bool _running = true;

    private static void _FileLoadCallback()
    {
        int? threadLimit = null;
        NativeInterop.ReadEnvironmentVariable("PCL_FILE_THREAD_LIMIT", ref threadLimit);
        
        // CPU 密集工作线程应使用性能内核的数量限制，防止跑到能效内核上
        // 如果这个死人调度还给往能效内核上扔就没法了，砍掉 Windows 即可解决
        threadLimit ??= NativeInterop.GetPerformanceLogicalProcessorCount();
        
        Context.Info($"以最多 {threadLimit} 个线程初始化线程池");
        var threadPool = new DualThreadPool((int)threadLimit);
        
        while (_running)
        {
            if (!_PendingTasks.TryDequeue(out var task))
            {
                _ContinueEvent.WaitOne();
                continue;
            }

            foreach (var item in task.Items)
            {
                var process = task.GetProcess(item) ?? _MatchDefaultProcess(item);
                var targetPath = item.TargetPath;
                if (File.Exists(targetPath)) PushProcess(targetPath);
                else
                {
                    var transfer = task.GetTransfer(item) ?? _MatchDefaultTransfer(item);
                    if (transfer == null) PushProcess(null);
                    else threadPool.QueueIo(() => transfer(item, PushProcess));
                }
                continue;

                void PushProcess(string? path)
                {
                    if (process == null) return;
                    threadPool.QueueCpu(() =>
                    {
                        var result = process(item, path);
                        task.OnProcessFinished(item, result);
                    });
                }
            }
        }
    }

    public static void QueueTask(IFileTask task)
    {
        _PendingTasks.Enqueue(task);
        _ContinueEvent.Set();
    }

    #endregion

}

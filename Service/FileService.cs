using System;
using System.IO;
using PCL.Core.Helper;
using PCL.Core.LifecycleManagement;
using Special = System.Environment.SpecialFolder;

namespace PCL.Core.Service;

[LifecycleService(LifecycleState.Loading, Priority = 1919820)]
public sealed class FileService : GeneralService
{
    private static LifecycleContext? _context;
    private static LifecycleContext Context => _context!;
    private FileService() : base("file", "文件管理") { _context = ServiceContext; }

    #region Paths

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
    
    public override void Start()
    {
        // correct shared paths
        const string name = "PCLCE";
        Context.Debug($"正在替换存储路径，目录名 {name}");
        SharedDataPath = GetSpecialPath(Special.ApplicationData, name);
        LocalDataPath = GetSpecialPath(Special.LocalApplicationData, name);
        TempPath = GetSpecialPath(Special.LocalApplicationData, $"Temp\\{name}");
#if DEBUG
        // read environments
        NativeInterop.ReadEnvironmentVariable("PCL_PATH", ref _dataPath);
        NativeInterop.ReadEnvironmentVariable("PCL_PATH_SHARED", ref _sharedDataPath);
        NativeInterop.ReadEnvironmentVariable("PCL_PATH_LOCAL", ref _localDataPath);
        NativeInterop.ReadEnvironmentVariable("PCL_PATH_TEMP", ref _tempPath);
#endif
    }
}

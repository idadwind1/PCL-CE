using System.Collections.Concurrent;

namespace PCL.Core.Utils.Download;

public class Downloader(int maxParallels = 16)
{
    /// <summary>
    /// 限制最大并发数量，该数量由所有实例共享。
    /// </summary>
    public static int ParallelTaskLimit { get; set; } = 16;
    
    /// <summary>
    /// 限制最大并发数量，该数量仅作用于该实例。
    /// </summary>
    public int MaxParallels { get; set; } = maxParallels;
    
    private ConcurrentQueue<DownloadItem> _downloadQueue = [];
}

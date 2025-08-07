using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using PCL.Core.Logging;

namespace PCL.Core.Network;

public class Downloader(int maxParallels = int.MaxValue, int? refreshInterval = null)
{
    private const string LogModule = "Downloader";

    private static int _schedulerCount = 0;
    
    /// <summary>
    /// 已实例化的调度器计数。该计数只与实例化行为有关，相关调度线程启动或停止均不会影响该计数。
    /// </summary>
    public static int SchedulerCount => _schedulerCount;
    
    /// <summary>
    /// 调度器默认刷新间隔。当没有为一个调度器指定 <see cref="RefreshInterval"/> 时，将使用此全局默认值。
    /// </summary>
    public static int DefaultRefreshInterval { get; set; } = 500;
    
    /// <summary>
    /// 限制全局最大并发数量。该数量由所有实例共享，更改后将于下一个任务分块生效。
    /// </summary>
    public static int ParallelTaskLimit { get; set; } = 16;

    private int? _refreshInterval = refreshInterval;

    /// <summary>
    /// 调度器刷新间隔。该间隔仅对该实例生效，未指定值时，将使用全局默认值 <see cref="DefaultRefreshInterval"/>。<br/>
    /// 使用 <see cref="ClearRefreshInterval"/> 可使此值变为未指定状态。
    /// </summary>
    public int RefreshInterval
    {
        get => _refreshInterval ?? DefaultRefreshInterval;
        set => _refreshInterval = value;
    }

    /// <summary>
    /// 清除调度器刷新间隔。清除后将回到未指定状态，并使用全局默认值。
    /// </summary>
    /// <returns>若原值为未指定状态则返回 <c>false</c>，否则返回 <c>true</c></returns>
    public bool ClearRefreshInterval()
    {
        if (_refreshInterval == null) return false;
        _refreshInterval = null;
        return true;
    }

    /// <summary>
    /// 限制最大并发数量。该数量仅作用于该实例，更改后将于下一个任务分块生效。
    /// </summary>
    public int MaxParallels { get; set; } = maxParallels;

    /// <summary>
    /// 当前实例的调度器 ID。
    /// </summary>
    public int SchedulerId { get; } = ++_schedulerCount;

    private ConcurrentQueue<DownloadItem> _downloadQueue = [];

    private Thread? _schedulerThread;

    public void AddItem(DownloadItem item)
    {
        LogWrapper.Trace(LogModule, $"#{SchedulerId} 新增任务: {item}");
        _downloadQueue.Enqueue(item);
    }
    
    private int _currentParallelCount = 0;
    
    private bool _CanStartNewParallel =>
        _currentParallelCount < MaxParallels && _currentParallelCount < ParallelTaskLimit;

    public void Start(CancellationToken cancelToken = default)
    {
        if (_schedulerThread != null) throw new InvalidOperationException("Cannot start twice");
        _schedulerThread = new Thread(() =>
        {
            var dequeued = new List<DownloadItem>();
            while (!cancelToken.IsCancellationRequested)
            {
                if (_CanStartNewParallel && _downloadQueue.TryDequeue(out var item))
                {
                    dequeued.Add(item);
                    if (item.Status == DownloadItemStatus.Waiting) _StartNewItem(item);
                    // TODO more processing
                }
                else
                {
                    if (cancelToken.IsCancellationRequested) break;
                    Thread.Sleep(RefreshInterval);
                    dequeued.ForEach(i => _downloadQueue.Enqueue(i));
                    dequeued.Clear();
                }
            }
        }) {
            IsBackground = true,
            Name = $"Scheduler@{SchedulerId}"
        };
        _schedulerThread.Start();
    }

    private void _StartNewItem(DownloadItem item)
    {
        throw new NotImplementedException();
    }
}

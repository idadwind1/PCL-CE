using System;
using System.Collections.Generic;
using System.IO;

namespace PCL.Core.Utils.Download;

public class DownloadItem(Uri uri, string targetPath, int chunkSize = 4096)
{
    public string TargetPath { get; } = Path.GetFullPath(targetPath);
    
    public Uri SourceUrl { get; set; } = uri;
    
    public long ContentLength { get; set; } = 0;
    
    public int ChunkSize { get; } = chunkSize;

    public LinkedList<ParallelTask> Parallels { get; } = [];

    public event Action? Finished;
    
    private int _finishedCount = 0;
    
    
}

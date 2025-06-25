using System;

namespace PCL.Core.Utils.FileVersionControl;

public struct FileVersionObjects
{
    public string Path {get;set;}
    public string Sha256 {get;set;}
    public long Length {get;set;}
    public DateTime CreationTime {get;set;}
    public DateTime LastWriteTime {get;set;}
}
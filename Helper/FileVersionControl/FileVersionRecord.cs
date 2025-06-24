namespace PCL.Core.Helper.FileVersionControl;

public struct FileVersionRecord
{
    public string Path {get;set;}
    public string Sha256 {get;set;}
    public long Length {get;set;}
}
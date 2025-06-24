using System;
using System.Collections.Generic;

namespace PCL.Core.Helper.FileVersionControl;

public struct VersionData
{
    /// <summary>
    /// ID 用 GUID
    /// </summary>
    public string NodeId {get;set;}
    public DateTime Created {get;set;}
    public List<FileVersionRecord> Records {get;set;}
    public string Name {get;set;}
    public string Desc {get;set;}
}
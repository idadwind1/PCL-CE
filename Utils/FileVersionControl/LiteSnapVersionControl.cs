using System;
using System.Collections.Generic;
using System.IO;
using LiteDB;

namespace PCL.Core.Utils.FileVersionControl;

/*
一个基于文件结构只记录差异文件的文件版本控制系统，用于 MC 存档备份
自己乱搓的，效率肯定没有 Git 高（
*/


public class LiteSnapVersionControl : IVersionControl , IDisposable
{
    private string _rootPath;
    private LiteDatabase _database;

    private const string ConfigFolderName = ".litesnap";
    private const string DatabaseName = "index.db";
    
    public LiteSnapVersionControl(string rootPath)
    {
        _rootPath = rootPath;
        var dbFile = Path.Combine(_rootPath, ConfigFolderName, DatabaseName);
        _database = new LiteDatabase($"Filename={dbFile}");
    }

    private bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _database?.Dispose();
    }
}
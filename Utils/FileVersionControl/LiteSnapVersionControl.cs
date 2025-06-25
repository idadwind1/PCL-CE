using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using PCL.Core.Helper;
using static System.IO.FileAttributes;
using Directory = System.IO.Directory;

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
    private const string DatabaseIndexTableName = "index";
    private const string ObjectsFolderName = "objects";
    private const string TempFolderName = "temp";
    
    public LiteSnapVersionControl(string rootPath)
    {
        _rootPath = rootPath;
        var dbFile = Path.Combine(_rootPath, ConfigFolderName, DatabaseName);
        var objFolder = Path.Combine(_rootPath, ConfigFolderName, ObjectsFolderName);
        if (!Directory.Exists(objFolder))
            Directory.CreateDirectory(objFolder);
        _database = new LiteDatabase($"Filename={dbFile}");
    }

    public async Task<string> CreateNewVersion()
    {
        var nodeId = Guid.NewGuid().ToString("N");
        var currentFiles = DirectoryHelper.EnumerateFiles(_rootPath, [ConfigFolderName]);
        currentFiles = currentFiles
            .Where(x => ((new FileInfo(x)).Attributes | Normal | Compressed | Archive | Hidden) == (Normal | Compressed | Archive | Hidden))
            .ToList();
        var hashComputedTasks = currentFiles.Select(currentFile => Task.Run(() =>
        {
            var fileInfo = new FileInfo(currentFile);
            using var fs = new FileStream(currentFile, FileMode.Open);
            var fileHash = HashHelper.ComputeSHA256(fs);
            return new FileVersionObjects()
            {
                Length = fs.Length,
                Path = currentFile.Replace(_rootPath, string.Empty).TrimStart(Path.DirectorySeparatorChar),
                Sha256 = fileHash,
                CreationTime = fileInfo.CreationTime,
                LastWriteTime = fileInfo.LastWriteTime,
            };
        })).ToArray();
        await Task.WhenAll(hashComputedTasks);
        var allFiles = hashComputedTasks.Select(x => x.Result);
        var changedFiles = hashComputedTasks
            .Select(x => x.Result)
            .Distinct(new FileVersionObjectsComparer())
            .Where(x => !HasFileObject(x.Sha256));
        var nodeObjects = _database.GetCollection<FileVersionObjects>(GetNodeTableNameById(nodeId));
        nodeObjects.InsertBulk(allFiles);

        var copyChangedFilesTasks = changedFiles.Select(x => Task.Run(async () =>
        {
            using var sourceFile = new FileStream(Path.Combine(_rootPath, x.Path), FileMode.Open);
            using var targetFile =
                new FileStream(Path.Combine(_rootPath, ConfigFolderName, ObjectsFolderName, x.Sha256), FileMode.Create,
                    FileAccess.ReadWrite, FileShare.Read);
            using var compressedTarget = new DeflateStream(targetFile, CompressionMode.Compress);
            await sourceFile.CopyToAsync(compressedTarget);
        }));
        await Task.WhenAll(copyChangedFilesTasks);
        
        var nodeList = _database.GetCollection<VersionData>(DatabaseIndexTableName);
        var currentNodeInfo = new VersionData()
        {
            Created = DateTime.Now,
            Desc = "A backup made by Plain Craft Launcher Community Edition",
            Name = $"{DateTime.Now:yyyyddMM}",
            NodeId = nodeId,
        };
        nodeList.Insert(currentNodeInfo);
        _database.Commit();
        return nodeId;
    }

    private string GetNodeTableNameById(string nodeId)
    {
        return $"node_{nodeId}";
    }

    private bool HasFileObject(string objectId)
    {
        return File.Exists(Path.Combine(_rootPath, ConfigFolderName, ObjectsFolderName, objectId));
    }

    public VersionData? GetVersion(string nodeId)
    {
        var nodeList = _database.GetCollection<VersionData>(DatabaseIndexTableName);
        return nodeList.FindOne(x => x.NodeId == nodeId);
    }

    public List<VersionData> GetVersions()
    {
        var nodeList = _database.GetCollection<VersionData>(DatabaseIndexTableName);
        return nodeList.Query().ToList();
    }

    public List<FileVersionObjects> GetObjects(string nodeId)
    {
        var objectList = _database.GetCollection<FileVersionObjects>(GetNodeTableNameById(nodeId));
        return objectList.Query().ToList();
    }

    public void DeleteVersion(string nodeId)
    {
        var nodeList = _database.GetCollection<VersionData>(DatabaseIndexTableName);
        nodeList.DeleteMany(x => x.NodeId == nodeId);
    }

    public Stream? GetObjectContent(string objectId)
    {
        var filePath = Path.Combine(_rootPath, ConfigFolderName, ObjectsFolderName, objectId);
        if (!HasFileObject(objectId))
            return null;
        return new DeflateStream(
            new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read),
            CompressionMode.Decompress);
    }

    public Task ApplyPastVersion(string nodeId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CheckVersion(string nodeId, bool deepCheck = false)
    {
        throw new NotImplementedException();
    }

    public Task CleanUnrecordObjects()
    {
        throw new NotImplementedException();
    }

    private bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _database?.Dispose();
    }
}
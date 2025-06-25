using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using PCL.Core.Helper;
using PCL.Core.Helper.Hash;
using static System.IO.FileAttributes;
using Directory = System.IO.Directory;

namespace PCL.Core.Utils.FileVersionControl;

/*
一个基于文件结构只记录差异文件的文件版本控制系统，用于 MC 存档备份
自己乱搓的，效率肯定没有 Git 高（
*/


public class SnapLiteVersionControl : IVersionControl , IDisposable
{
    private readonly string _rootPath;
    private readonly LiteDatabase _database;
    private readonly IHashProvider _hash;

    private const string ConfigFolderName = ".litesnap";
    private const string DatabaseName = "index.db";
    private const string DatabaseIndexTableName = "index";
    private const string ObjectsFolderName = "objects";
    private const string TempFolderName = "temp";
    
    public SnapLiteVersionControl(string rootPath)
    {
        try
        {
            _rootPath = rootPath;
            _hash = new SHA512Provider();
            var dbFile = Path.Combine(_rootPath, ConfigFolderName, DatabaseName);
            var objFolder = Path.Combine(_rootPath, ConfigFolderName, ObjectsFolderName);
            if (!Directory.Exists(objFolder))
                Directory.CreateDirectory(objFolder);
            _database = new LiteDatabase($"Filename={dbFile}");
        }
        catch (Exception e)
        {
            LogWrapper.Error(e,$"无法加载位于 {_rootPath} 处的 SnapLite 数据");
            throw;
        }

    }

    private async Task<FileVersionObjects[]> GetAllTrackedFiles()
    {
        var currentFiles = DirectoryHelper.EnumerateFiles(_rootPath, [ConfigFolderName]);
        currentFiles = currentFiles
            .Where(x => ((new FileInfo(x)).Attributes | Normal | Compressed | Archive | Hidden) == (Normal | Compressed | Archive | Hidden))
            .ToList();
        var ret = currentFiles.Select(currentFile => Task.Run(() =>
        {
            var fileInfo = new FileInfo(currentFile);
            using var fs = new FileStream(currentFile, FileMode.Open);
            var fileHash = _hash.ComputeHash(fs);
            return new FileVersionObjects()
            {
                Length = fs.Length,
                Path = currentFile.Replace(_rootPath, string.Empty).TrimStart(Path.DirectorySeparatorChar),
                Hash = fileHash,
                CreationTime = fileInfo.CreationTime,
                LastWriteTime = fileInfo.LastWriteTime,
            };
        })).ToArray();
        await Task.WhenAll(ret);
        return ret.Select(x => x.Result).ToArray();
    }

    public async Task<string> CreateNewVersion()
    {
        var nodeId = Guid.NewGuid().ToString("N");
        var allFiles = await GetAllTrackedFiles();
        var newAddFiles = allFiles
            .Distinct(new FileVersionObjectsComparer())
            .Where(x => !HasFileObject(x.Hash));
        var nodeObjects = _database.GetCollection<FileVersionObjects>(GetNodeTableNameById(nodeId));
        nodeObjects.InsertBulk(allFiles);

        var copyNewFilesTasks = newAddFiles.Select(x => Task.Run(async () =>
        {
            using var sourceFile = new FileStream(Path.Combine(_rootPath, x.Path), FileMode.Open);
            using var targetFile =
                new FileStream(Path.Combine(_rootPath, ConfigFolderName, ObjectsFolderName, x.Hash), FileMode.Create,
                    FileAccess.ReadWrite, FileShare.Read);
            using var compressedTarget = new DeflateStream(targetFile, CompressionMode.Compress);
            await sourceFile.CopyToAsync(compressedTarget);
        }));
        await Task.WhenAll(copyNewFilesTasks);
        
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

    private static string GetNodeTableNameById(string nodeId)
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

    public List<FileVersionObjects> GetNodeObjects(string nodeId)
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

    public async Task Export(string nodeId, string saveFilePath)
    {
        var fileObjects = GetNodeObjects(nodeId);
        if (File.Exists(saveFilePath))
            File.Delete(saveFilePath);
        using var fs = new FileStream(saveFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        using var targetZip = new ZipArchive(fs, ZipArchiveMode.Update);
        foreach (var fileObject in fileObjects)
        {
            await Task.Run(async () =>
            {
                var entry = targetZip.CreateEntry(fileObject.Path);
                entry.LastWriteTime = fileObject.LastWriteTime;
                using var writer = entry.Open();
                using var objectReader =
                    new FileStream(Path.Combine(_rootPath, ConfigFolderName, ObjectsFolderName, fileObject.Hash),
                        FileMode.Open,
                        FileAccess.Read, FileShare.Read);
                using var reader = new DeflateStream(objectReader, CompressionMode.Decompress);
                await reader.CopyToAsync(writer);
            });
        }
    }

    private bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _database?.Dispose();
    }
}
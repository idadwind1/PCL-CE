using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using PCL.Core.Model;

namespace PCL.Core.Helper
{
    public class JavaManage
    {
        private List<Java> _javas = new List<Java>();
        public List<Java> JavaList
        {
            get { return _javas.ToList(); }
        }

        public async Task ScanJava()
        {
            var javaList = new List<Java>();
            var javaPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var SearchTasks = new List<Task>();
            var Searchers = new TaskFactory();
            SearchTasks.Add(Searchers.StartNew(() => ScanRegistryForJava(ref javaPaths)));
            SearchTasks.Add(Searchers.StartNew(() => ScanDefaultInstallPaths(ref javaPaths)));
            SearchTasks.Add(Searchers.StartNew(() => ScanPathEnvironmentVariable(ref javaPaths)));
            SearchTasks.Add(Searchers.StartNew(() => ScanMicrosoftStoreJava(ref javaPaths)));
            await Searchers.ContinueWhenAll(SearchTasks.ToArray(), completedTask => { });

            foreach (var javaExePath in javaPaths)
            {
                var javaModel = Java.Prase(javaExePath);
                if (javaModel != null)
                {
                    javaList.Add(javaModel);
                }
            }
            
            _javas = (from j in javaList
                      orderby j.Version descending, j.Brand
                      select j).ToList();
        }

        public async Task<List<Java>> SelectSuitableJava(Version MinVerison, Version MaxVersion)
        {
            if (_javas == null || _javas.Count == 0)
                await ScanJava();
            return (from j in _javas
                    where j.IsEnabled && j.Version >= MinVerison && j.Version <= MaxVersion
                    orderby j.Version, j.Brand
                    select j).ToList();
        }

        private void ScanRegistryForJava(ref HashSet<string> javaPaths)
        {
            var registryPaths = new List<string>
            {
                @"SOFTWARE\JavaSoft\Java Development Kit",
                @"SOFTWARE\JavaSoft\Java Runtime Environment",
                @"SOFTWARE\WOW6432Node\JavaSoft\Java Development Kit",
                @"SOFTWARE\WOW6432Node\JavaSoft\Java Runtime Environment"
            };

            foreach (var regPath in registryPaths)
            {
                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(regPath))
                {
                    if (regKey != null)
                    {
                        foreach (var subKeyName in regKey.GetSubKeyNames())
                        {
                            using (RegistryKey subKey = regKey.OpenSubKey(subKeyName))
                            {
                                string javaHome = subKey?.GetValue("JavaHome") as string;
                                if (!string.IsNullOrEmpty(javaHome))
                                {
                                    string javaExePath = Path.Combine(javaHome, "bin\\java.exe");
                                    if (File.Exists(javaExePath))
                                    {
                                        javaPaths.Add(javaExePath);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ScanDefaultInstallPaths(ref HashSet<string> javaPaths)
        {
            // 准备欲搜索目录
            var programFilesPaths = new List<string>
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            };
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string[] keyFolders =
                {
                    "Program Files",
                    "Program Files (x86)",
                    "Programs"
                };
                foreach (var driver in DriveInfo.GetDrives())
                {
                    foreach (var keyFolder in keyFolders)
                    {
                        programFilesPaths.Add(Path.Combine(driver.Name, keyFolder));
                    }
                }
            }
            else
            {
                programFilesPaths.AddRange(new List<string> {
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
                });
            }
            programFilesPaths = programFilesPaths
                .Where(x => !string.IsNullOrEmpty(x) && Directory.Exists(x))
                .ToList();

            // 可能的目录关键词列表
            string[] keyWord =
            {"java", "jdk", "jre",
            "dragonwell", "zulu", "oracle", "open", "corretto", "eclipse", "hotspot", "semeru", "kona",
            "environment", "env", "runtime", "x86_64", "amd64", "arm64",
            "pcl", "hmcl", "baka", "minecraft"};

            // 最大文件夹搜索深度
            const int MAX_SEARCH_DEPTH = 12;

            // 使用 广度优先搜索 查找 Java 文件
            foreach (var rootPath in programFilesPaths)
            {
                var queue = new Queue<(string path, int depth)>();
                queue.Enqueue((rootPath, 0));
                while (queue.Count > 0)
                {
                    var (currentPath, depth) = queue.Dequeue();
                    if (depth > MAX_SEARCH_DEPTH) continue;
                    try
                    {
                        // 只遍历包含关键字的目录
                        var subDirs = Directory.EnumerateDirectories(currentPath)
                            .Where(x => keyWord.Any(k => x.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0));
                        foreach (var dir in subDirs)
                        {
                            // 准备可能的 Java 路径
                            var potentialJavas = new List<string>
                            {
                                Path.Combine(dir, "bin", "java.exe"),
                                Path.Combine(dir, "jre", "bin", "java.exe")
                            };
                            potentialJavas = potentialJavas
                                .Where(File.Exists)
                                .ToList();
                            // 存在 Java，节点达到目标
                            if (potentialJavas.Any())
                            {
                                foreach (var javaPath in potentialJavas)
                                {
                                    if (File.Exists(javaPath))
                                        javaPaths.Add(javaPath);
                                }
                            }
                            else
                            {
                                queue.Enqueue((dir, depth + 1));
                            }
                        }
                    }
                    catch { /* 忽略无权限等异常 */ }
                }
            }
        }

        private void ScanPathEnvironmentVariable(ref HashSet<string> javaPaths)
        {
            string pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathEnv)) return;

            string[] paths = pathEnv.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var targetPath in paths)
            {
                string javaExePath = Path.Combine(targetPath, "java.exe");
                if (File.Exists(javaExePath))
                {
                    javaPaths.Add(javaExePath);
                }
            }
        }

        private void ScanMicrosoftStoreJava(ref HashSet<string> javaPaths)
        {
            var MsJavaFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Packages",
                "Microsoft.4297127D64EC6_8wekyb3d8bbwe", // Ms Java 的固定下载地址
                "LocalCache",
                "Local",
                "runtime");
            if (!Directory.Exists(MsJavaFolder))
                return;
            // 搜索第一级目录：以"java-runtime"开头的文件夹
            foreach (var runtimeDir in Directory.EnumerateDirectories(MsJavaFolder))
            {
                string dirName = Path.GetFileName(runtimeDir);
                if (!dirName.StartsWith("java-runtime"))
                    continue;

                // 搜索第二级目录：平台架构目录 (如 windows-x64)
                foreach (var archDir in Directory.EnumerateDirectories(runtimeDir))
                {
                    // 搜索第三级目录：具体运行时版本目录
                    foreach (var versionDir in Directory.EnumerateDirectories(archDir))
                    {
                        // 检查bin/java.exe是否存在
                        string javaExePath = Path.Combine(versionDir, "bin", "java.exe");
                        if (File.Exists(javaExePath))
                        {
                            javaPaths.Add(javaExePath);
                        }
                    }
                }
            }
        }
    }
}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PCL.Core.Model;
using System.Net.NetworkInformation;

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
            
            _javas = javaList
                .OrderByDescending(x => x.Version)
                .ToList();
        }

        public async Task<List<Java>> SelectSuitableJava(Version MinVerison, Version MaxVersion)
        {
            if (_javas == null || _javas.Count == 0)
                await ScanJava();
            return _javas
                .Where(java => java.Version >= MinVerison && java.Version <= MaxVersion)
                .OrderByDescending(java => java.Version)
                .ToList();
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
            var programFilesPaths = new List<string>
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                Environment.GetFolderPath(Environment.SpecialFolder.Programs),
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            };

            foreach (var pfPath in programFilesPaths)
            {
                string javaDir = Path.Combine(pfPath, "Java");
                if (Directory.Exists(javaDir))
                {
                    foreach (var dirPath in Directory.GetDirectories(javaDir))
                    {
                        string[] potentialJavas =
                        {
                            Path.Combine(dirPath, "bin", "java.exe"),
                            Path.Combine(dirPath, "jre", "bin", "java.exe")
                        };
                        var existingJavas = potentialJavas
                            .Where(File.Exists)
                            .ToList();
                        foreach (var item in existingJavas)
                        {
                            javaPaths.Add(item);
                        }
                    }
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
            //TODO: 扫描  Microsoft Java
        }
    }
}

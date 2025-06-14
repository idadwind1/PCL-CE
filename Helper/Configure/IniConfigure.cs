using PCL.Core.Interface;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;

namespace PCL.Core.Helper.Configure
{
    public class IniConfigure : IConfigure
    {
        private ConcurrentDictionary<string, string> _content;
        
        private readonly string _path;
        private readonly object _fileOpLock = new object();
        public IniConfigure(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            _path = path;
            _load();
            _content ??= new();
        }

        private void _load()
        {
            lock (_fileOpLock)
            {
                var folder = _path.Substring(0, _path.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                using var fs = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                using var reader = new StreamReader(fs);
                _content = new();
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var splitPos = line.IndexOf(':');
                    var key = line.Substring(0, splitPos);
                    var value = line.Substring(splitPos + 1);
                    _content.TryAdd(key, value);
                }
            }
        }
        
        public void Set(string key, object value)
        {
            if (key.Contains(Environment.NewLine))
                throw new ArgumentException(nameof(key));
            _content.AddOrUpdate(
                key,
                _ => Convert.ToBase64String(Encoding.UTF8.GetBytes(value.ToString())),
                (_, _) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value.ToString())));
            Flush();
        }
        
        public TValue? Get<TValue>(string key)
        {
            try
            {
                if (_content.TryGetValue(key, out string? ret))
                {
                    return (TValue)Convert.ChangeType(Encoding.UTF8.GetString(Convert.FromBase64String(ret)), typeof(TValue));
                }
            }
            catch { }
            return default;
        }
        
        public bool Contains(string key)
        {
            return _content.ContainsKey(key);
        }

        public void Clear()
        {
            _content.Clear();
            Flush();
        }

        public void Remove(string key)
        {
            _content.TryRemove(key, out _);
            Flush();
        }
        
        public void Flush()
        {
            lock (_fileOpLock)
            {
                using var fs = new FileStream($"{_path}.temp", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                using var writer = new StreamWriter(fs);
                foreach (var item in _content)
                {
                    writer.WriteLine($"{item.Key}:{item.Value}");
                }
                writer.Close();
                fs.Close();
                File.Replace($"{_path}.temp", _path, $"{_path}.bak");
            }
        }

        public void Reload()
        {
            _load();
        }
    }
}

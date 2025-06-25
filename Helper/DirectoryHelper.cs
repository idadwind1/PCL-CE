using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PCL.Core.Helper;

public static class DirectoryHelper
{
    public static List<string> EnumerateFiles(string directory, List<string>? ignoreDirectories = null)
    {
        if (ignoreDirectories == null)
            return [..Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)];
        List<string> ret = [];
        ret.AddRange(Directory.EnumerateFiles(directory));
        foreach (var dirs in Directory.EnumerateDirectories(directory))
        {
            if (ignoreDirectories.Contains(dirs.Split(Path.DirectorySeparatorChar).Last()))
                continue;
            ret.AddRange(Directory.EnumerateFiles(dirs, "*", SearchOption.AllDirectories));
        }
        return ret;
    }
}
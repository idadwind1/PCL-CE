using System;
using System.Collections.Generic;

namespace PCL.Core.Utils.FileVersionControl;

public class FileVersionObjectsComparer : IEqualityComparer<FileVersionObjects>
{
    public bool Equals(FileVersionObjects x, FileVersionObjects y)
    {
        return x.Hash == y.Hash;
    }

    public int GetHashCode(FileVersionObjects obj)
    {
        return obj.Hash.GetHashCode();
    }
}
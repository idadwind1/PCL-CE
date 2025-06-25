using System;
using System.Collections.Generic;

namespace PCL.Core.Utils.FileVersionControl;

public class FileVersionObjectsComparer : IEqualityComparer<FileVersionObjects>
{
    public bool Equals(FileVersionObjects x, FileVersionObjects y)
    {
        return x.Sha256 == y.Sha256;
    }

    public int GetHashCode(FileVersionObjects obj)
    {
        return obj.Sha256.GetHashCode();
    }
}
using System.Threading.Tasks;

namespace PCL.Core.VersionControl;

public interface IBinaryDiff
{
    public Task<byte[]> Make(byte[] originData, byte[] newData);
    public Task<byte[]> Apply(byte[] originData, byte[] diffData);
}
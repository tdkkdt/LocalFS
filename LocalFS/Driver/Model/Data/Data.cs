using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LocalFS.Driver.Model.ClustersAllocator;

namespace LocalFS.Driver.Model.Data {
    internal interface Data {
        public int Length { get; }
        Task<int> ReadData(long position, byte[] data, int offset, int length, CancellationToken ct);
        Task<Data> MutateLength(long newLength, ClustersTransaction tx, CancellationToken ct);
        Task WriteData(long position, byte[] data, int offset, int length, CancellationToken ct);
        void Write(BinaryWriter binaryWriter);
    }
}
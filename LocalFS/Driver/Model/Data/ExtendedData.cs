using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LocalFS.Driver.Model.ClustersAllocator;

namespace LocalFS.Driver.Model.Data {
    internal class ExtendedData:Data {
        public int Length { get; }        
        public static ExtendedData Read(FileSystem fileSystem, BinaryReader binaryReader) {
            throw new System.NotImplementedException();
        }
        
        public Task<int> ReadData(long position, byte[] data, int offset, int length, CancellationToken ct) {
            throw new System.NotImplementedException();
        }

        public Task<Data> MutateLength(long newLength, ClustersTransaction tx, CancellationToken ct) {
            throw new System.NotImplementedException();
        }

        public Task WriteData(long position, byte[] data, int offset, int length, CancellationToken ct) {
            throw new System.NotImplementedException();
        }

        public void Write(BinaryWriter binaryWriter) {
            throw new System.NotImplementedException();
        }
    }
}
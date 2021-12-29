using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LocalFS.Driver.Model.ClustersAllocator;

namespace LocalFS.Driver.Model.Data {
    internal class ResidentData : Data {
        public int Length => Inner.Length;

        private FileSystem FileSystem { get; }
        private byte[] Inner { get; }

        public ResidentData(FileSystem fileSystem, byte[] inner) {
            Inner = inner;
            FileSystem = fileSystem;
        }

        public static Data Read(FileSystem fileSystem, BinaryReader binaryReader) {
            int length = binaryReader.ReadInt32();
            byte[] data = binaryReader.ReadBytes(length);
            return new ResidentData(fileSystem, data);
        }

        public Task<int> ReadData(long position, byte[] data, int offset, int length, CancellationToken ct) {
            if (position >= Inner.Length) {
                return Task.FromResult(0);
            }
            int effectiveLength = (int)Math.Min(length, Inner.Length - position);
            Array.Copy(Inner, position, data, offset, effectiveLength);
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(effectiveLength);
        }

        public Task<Data> MutateLength(long newLength, ClustersTransaction tx, CancellationToken ct) {
            if (newLength <= FileSystem.FileSystemInfo.ClusterSize - Entry.MAXIMUM_LENGTH) {
                // можно продолжать использовать резидентную дату
                var newInner = new byte[newLength];
                Array.Copy(Inner, newInner, Math.Min(newLength, Length));
                ct.ThrowIfCancellationRequested();
                Data newData = new ResidentData(FileSystem, newInner);
                return Task.FromResult(newData);
            }
            throw new NotImplementedException();
        }

        public Task WriteData(long position, byte[] data, int offset, int length, CancellationToken ct) {
            Array.Copy(data, offset, Inner, position, length);
            ct.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public void Write(BinaryWriter binaryWriter) {
            binaryWriter.Write(Inner.Length);
            binaryWriter.Write(Inner);
        }
    }
}
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LocalFS.Driver.Model.Data;

namespace LocalFS.Driver.Model {
    internal class EntriesManager {
        //TODO прикрутить LRU кэш
        private FileSystem FileSystem { get; }

        public EntriesManager(FileSystem fileSystem) {
            FileSystem = fileSystem;
        }

        internal async Task<Entry> GetFileEntry(int clusterIndex, CancellationToken ct) {
            if (FileSystem.ClustersAllocator.IsFreeCluster(clusterIndex)) {
                throw new ConsistencyException("Cluster {} shouldn't be empty");
            }
            var clusterData = new byte[FileSystem.FileSystemInfo.ClusterSize]; //TODO array pool / Memory
            var readTask = FileSystem.ReadController.AddTask(
                clusterData,
                clusterIndex * 1L * FileSystem.FileSystemInfo.ClusterSize,
                ct
            );
            await readTask;
            return ReadEntry(FileSystem, clusterData);
        }

        internal async Task SaveFileEntryAsync(int clusterIndex, Entry entry) {
            byte[] clusterData = PrepareEntryForSaving(clusterIndex, entry);
            var writeTask = FileSystem.WriteController.AddTask(
                clusterData,
                FileSystem.FileSystemInfo.OffsetByCluster(clusterIndex),
                CancellationToken.None
            );
            await writeTask;
        }

        private byte[] PrepareEntryForSaving(int clusterIndex, Entry entry) {
            if (FileSystem.ClustersAllocator.IsFreeCluster(clusterIndex)) {
                throw new ConsistencyException("Cluster {} shouldn't be empty");
            }
            return WriteEntry(entry);
        }

        internal void SaveFileEntry(int clusterIndex, Entry entry) {
            var clusterData = PrepareEntryForSaving(clusterIndex, entry);
            FileSystem.WriteController.WriteNow(
                clusterData,
                FileSystem.FileSystemInfo.OffsetByCluster(clusterIndex)
            );
        }

        private static Entry ReadEntry(FileSystem fileSystem, byte[] clusterData) {
            using var binaryReader = new BinaryReader(new MemoryStream(clusterData), Encoding.UTF8, false);
            int index = binaryReader.ReadInt32();
            string name = binaryReader.ReadString();
            int typeI = binaryReader.ReadInt32();
            var creationTime = new DateTime(binaryReader.ReadInt64());
            var modificationTime = new DateTime(binaryReader.ReadInt64());
            var data = DataManager.ReadData(fileSystem, binaryReader);
            return new Entry(
                index,
                name,
                (EntryType)typeI,
                creationTime,
                modificationTime,
                data
            );
        }

        private byte[] WriteEntry(Entry entry) {
            var bytes = new byte[FileSystem.FileSystemInfo.ClusterSize];
            using var binaryWriter = new BinaryWriter(new MemoryStream(bytes), Encoding.UTF8, false);
            binaryWriter.Write(entry.Index);
            binaryWriter.Write(entry.Name);
            binaryWriter.Write((int)entry.Type);
            binaryWriter.Write(entry.CreationTime.Ticks);
            binaryWriter.Write(entry.ModificationTime.Ticks);
            DataManager.WriteData(entry.Data, binaryWriter);
            return bytes;
        }

        public Entry CreateEmptyFile(int index, string name) {
            return new Entry(
                index,
                name,
                EntryType.FILE,
                DateTime.UtcNow,
                DateTime.UtcNow,
                new ResidentData(FileSystem, Array.Empty<byte>())
            );
        }
    }
}
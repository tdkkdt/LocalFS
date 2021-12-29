using System;
using System.Threading.Tasks;
using LocalFS.Driver.Model.ClustersAllocator;

namespace LocalFS.Driver.Model {
    public class DirectoryHandle : IDisposable {
        private FileSystem FileSystem { get; }
        public void Dispose() {
            throw new NotImplementedException();
        }

        internal async Task<Entry> AddFile(string name) {
            using ClustersTransaction tx = FileSystem.ClustersAllocator.OpenTransaction();
            var clusterIndex = FileSystem.ClustersAllocator.AllocateEntry(tx);
            Entry entry = FileSystem.EntriesManager.CreateEmptyFile(clusterIndex, name);
            await FileSystem.EntriesManager.SaveFileEntryAsync(
                clusterIndex,
                entry
            );
            AddFileToDirEntry(name);
            await tx.Commit();
            return entry;
        }
    }
}
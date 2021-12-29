using LocalFS.Driver.API;
using LocalFS.Driver.Model.IOSubsystem;

namespace LocalFS.Driver.Model {
    internal class FileSystem {
        internal ReadController ReadController { get; }
        internal WriteController WriteController { get; }
        internal EntriesManager EntriesManager { get; }
        internal FileSystemInfo FileSystemInfo { get; }
        internal ClustersAllocator.ClustersAllocator ClustersAllocator { get; }
        internal FileHandlesLocksManager FileHandlesLocksManager { get; }

        public void CloseFileHandle(FileHandle fileHandle) {
            throw new System.NotImplementedException();
        }
    }
}
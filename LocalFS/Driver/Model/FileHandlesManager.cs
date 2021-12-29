using System;
using System.Threading;
using System.Threading.Tasks;
using LocalFS.Driver.API;

namespace LocalFS.Driver.Model {
    internal class FileHandlesManager {
        private long indexCounter; // TODO нужна проверка, что такой хэндл ещё не используется
        private FileSystem FileSystem { get; }

        public async Task<FileHandle> OpenFileRead(Path path, CancellationToken ct) { 
            using DirectoryHandle dirHandle = await OpenDirectoryExclusive(path, ct);
            var fileClusterIndex = dirHandle.GetClusterIndexForChild(path.Tokens[^1]);
            long handleIndex = NextHandleIndex();
            Entry entry = await FileSystem.EntriesManager.GetFileEntry(fileClusterIndex, ct);
            if (entry.Type != EntryType.FILE) {
                throw new ApiException($"{path} is not a path for a file");
            }
            IDisposable fhLock = FileSystem.FileHandlesLocksManager.OpenRead(fileClusterIndex, handleIndex);
            dirHandle.Dispose();
            return new FileHandle(
                FileSystem,
                handleIndex,
                fileClusterIndex,
                false,
                fhLock
            );
        }

        public async Task<FileHandle> OpenFileWrite(Path path, CancellationToken ct) {
            using DirectoryHandle dirHandle = OpenDirectoryExclusive(path);
            var fileClusterIndex = dirHandle.GetClusterIndexForChild(path.Tokens[^1]);
            long handleIndex = NextHandleIndex();
            Entry entry = await FileSystem.EntriesManager.GetFileEntry(fileClusterIndex, ct);
            if (entry.Type != EntryType.FILE) {
                throw new ApiException($"{path} is not a path for a file");
            }
            if (!FileSystem.FileHandlesLocksManager.TryOpenWrite(fileClusterIndex, handleIndex, out IDisposable? fhLock)) {
                throw new ApiException("File is already open to write");
            }
            if (fhLock == null) {
                throw new InternalIOException("fhLock is null!!!", null);
            }

            return new FileHandle(
                FileSystem,
                handleIndex,
                fileClusterIndex,
                true,
                fhLock
            );
        }

        public async Task<FileHandle> CreateFile(Path path) {
            using DirectoryHandle dirHandle = OpenDirectoryExclusive(path);
            string name = path.Tokens[^1];
            if (dirHandle.Contains(name)) {
                throw new ApiException($"The path {path} is already busy");
            }
            long handleIndex = NextHandleIndex();
            var entry = await dirHandle.AddFile(name);
            IDisposable fhLock = FileSystem.FileHandlesLocksManager.OpenWrite(entry.Index, handleIndex, CancellationToken.None);
            return new FileHandle(
                FileSystem,
                handleIndex,
                entry.Index,
                true,
                fhLock
            );
        }

        public async Task DeleteFile(Path path, CancellationToken ct) {
            using DirectoryHandle dirHandle = OpenDirectoryExclusive(path);
            string fileName = path.Tokens[^1];
            var fileClusterIndex = dirHandle.GetClusterIndexForChild(fileName);
            var entry = await FileSystem.EntriesManager.GetFileEntry(fileClusterIndex, ct);
            if (entry.Type != EntryType.FILE) {
                throw new ApiException($"{path} is not a path for a file");
            }
            await dirHandle.RemoveFile(fileName);
        }

        private long NextHandleIndex() {
            return Interlocked.Increment(ref indexCounter);
        }

        private DirectoryHandle OpenDirectoryExclusive(Path path) {
            var prevHandle = OpenRootWrite();
            for (int i = 0; i < path.Tokens.Length - 1) {
                var curHandle = OpenChildWrite(prevHandle, path.Tokens[i]);
                prevHandle.Dispose();
                prevHandle = curHandle;
            }
            return prevHandle;
        }
    }
}
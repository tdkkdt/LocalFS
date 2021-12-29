using System;
using System.Threading;
using System.Threading.Tasks;
using LocalFS.Driver.Model;
using LocalFS.Driver.Model.Data;

namespace LocalFS.Driver.API {
    //TODO в диспоузе и финализаторе надо чистить RW локи, которые открыты в этом файле
    public class FileHandle : IDisposable {
        public long HandleIndex { get; }
        private FileSystem FileSystem { get; }
        private int EntryClusterIndex { get; }
        public long Position { get; private set; }
        public long Length { get; private set; }
        public bool CanWrite { get; }
        private bool Disposed { get; set; }
        private IDisposable FHLock { get; }

        internal FileHandle(FileSystem fileSystem, long handleIndex, int entryClusterIndex, bool canWrite, IDisposable fhLock) {
            FileSystem = fileSystem;
            EntryClusterIndex = entryClusterIndex;
            CanWrite = canWrite;
            Disposed = false;
            HandleIndex = handleIndex;
            FHLock = fhLock;
        }

        public void SetPosition(long newPosition) {
            CheckDisposed();
            Position = newPosition;
        }

        public async Task SetLength(long newLength, CancellationToken ct) {
            CheckDisposed();
            CheckWritePermissions();
            if (newLength < 0) {
                throw new ArgumentOutOfRangeException(nameof(newLength), newLength, "Should be positive");
            }
            var entry = await FileSystem.EntriesManager.GetFileEntry(EntryClusterIndex, ct);
            using var tx = FileSystem.ClustersAllocator.OpenTransaction();
            Data newData = await entry.Data.MutateLength(newLength, tx, ct);
            CheckNewLength(newLength, newData);
            var newEntry = new Entry(
                entry.Index,
                entry.Name,
                entry.CreationTime,
                DateTime.UtcNow,
                newData
            );
            ct.ThrowIfCancellationRequested();
            // Следующие две строчки - ужасные точки отказа. Они должны выполниться максимально атомарно.
            // чтобы было менее страшно хорошо бы здесь прикрутить журналирование
            await FileSystem.EntriesManager.SaveFileEntryAsync(EntryClusterIndex, newEntry);
            await tx.Commit();
            Position = Math.Min(Position, Length);
            Length = newEntry.Data.Length;
        }
        
        public async Task<int> Read(byte[] data, int offset, int length, CancellationToken ct) {
            CheckDisposed();
            CheckDataArray(data, offset, length);
            var entry = await FileSystem.EntriesManager.GetFileEntry(EntryClusterIndex, ct);
            int bytesRead = await entry.Data.ReadData(Position, data, offset, length, ct);
            Position += bytesRead;
            Length = entry.Data.Length;
            return bytesRead;
        }

        public async Task Write(byte[] data, int offset, int length, CancellationToken ct) {
            CheckDisposed();
            CheckWritePermissions();
            CheckDataArray(data, offset, length);
            var entry = await FileSystem.EntriesManager.GetFileEntry(EntryClusterIndex, ct);
            using var tx = FileSystem.ClustersAllocator.OpenTransaction();
            var newData = entry.Data;
            if (entry.Data.Length < Position + length) {
                long newLength = Position + length;
                newData = await entry.Data.MutateLength(newLength, tx, ct);
                CheckNewLength(newLength, newData);
            }
            await newData.WriteData(Position, data, offset, length, ct);
            var newEntry = new Entry(
                entry.Index,
                entry.Name,
                entry.CreationTime,
                DateTime.UtcNow,
                newData
            );
            ct.ThrowIfCancellationRequested();
            // Следующие две строчки - ужасные точки отказа. Они должны выполниться максимально атомарно.
            // чтобы было менее страшно хорошо бы здесь прикрутить журналирование
            await FileSystem.EntriesManager.SaveFileEntryAsync(EntryClusterIndex, newEntry);
            await tx.Commit();
            Position += length;
            Length = newEntry.Data.Length;
        }

        private static void CheckDataArray(byte[] data, int offset, int length) {
            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }
            if (offset < 0 || length < 0) {
                throw new ArgumentException("Offset and length should be positive numbers");
            }
            if (offset + length > data.Length) {
                throw new ArgumentException("Data size should be greater then or equals to offset + length");
            }
        }

        private static void CheckNewLength(long newLength, Data newData) {
            if (newData.Length != newLength) {
                throw new ConsistencyException("Length doesn't changes correctly");
            }
        }

        private void CheckDisposed() {
            if (Disposed) {
                throw new ObjectDisposedException(nameof(FileHandle));
            }
        }

        private void CheckWritePermissions() {
            if (!CanWrite) {
                throw new InvalidOperationException("FileHandle doesn't have permissions to write.");
            }
        }

        public void Dispose() {
            if (Disposed) {
                return;
            }
            FHLock.Dispose();
            FileSystem.CloseFileHandle(this);
            Disposed = true;
        }
    }
}
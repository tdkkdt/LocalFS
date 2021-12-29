using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace LocalFS.Driver.Model.IOSubsystem {
    public sealed class WriteController : IDisposable {
        private bool WasDisposed { get; set; }
        private BlockingCollection<WriteCommand> Inner { get; }
        private System.IO.FileStream Stream { get; }
        private object WriteLocker { get; }

        public WriteController(System.IO.FileStream stream, int collectionSize) {
            Stream = stream;
            Inner = new BlockingCollection<WriteCommand>(collectionSize);
            WriteLocker = new object();
        }

        public void DoRoutine(CancellationToken ct) {
            while (!Inner.IsCompleted) {
                if (WasDisposed) {
                    throw new ObjectDisposedException("WriteController");
                }

                WriteCommand command;
                try {
                    command = Inner.Take(ct);
                }
                catch (OperationCanceledException e) {
                    return;
                }

                lock (WriteLocker) {
                    if (command.TCS.Task.IsCanceled) {
                        continue;
                    }
                    try {
                        Stream.Position = command.Offset;
                        Stream.Write(command.Data, 0, command.Data.Length);
                        command.TCS.TrySetResult(true);
                    }
                    catch (Exception e) {
                        command.TCS.TrySetException(new InternalIOException("An exception occured while writing data", e));
                    }
                }
            }
        }

        public Task<bool> AddTask(byte[] data, long offset, CancellationToken ct) {
            if (WasDisposed) {
                throw new ObjectDisposedException("WriteController");
            }
            var command = new WriteCommand(offset, data);
            ct.Register(() => command.TCS.SetCanceled(ct));
            Inner.TryAdd(command, -1, ct);
            return command.TCS.Task;
        }

        public void Dispose() {
            WasDisposed = true;
            Inner?.Dispose();
        }

        public void WriteNow(byte[] bytes, long offset) {
            lock (WriteLocker) {
                try {
                    Stream.Position = offset;
                    Stream.Write(bytes, 0, bytes.Length);
                }
                catch (Exception e) {
                    throw new InternalIOException("An exception occured while writing data", e);
                }
            }
        }
    }
}
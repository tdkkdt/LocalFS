using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace LocalFS.Driver.Model.IOSubsystem {
    public sealed class ReadController :IDisposable {
        private bool WasDisposed { get; set; }
        private BlockingCollection<ReadCommand> Inner { get; }
        private System.IO.FileStream Stream { get; }

        public ReadController(System.IO.FileStream stream, int collectionSize) {
            Stream = stream;
            Inner = new BlockingCollection<ReadCommand>(collectionSize);
        }

        public void DoRoutine(CancellationToken ct) {
            while (!Inner.IsCompleted) {
                if (WasDisposed) {
                    throw new ObjectDisposedException("ReadController");
                }

                ReadCommand command;
                try {
                    command = Inner.Take(ct);
                }
                catch (OperationCanceledException e) {
                    return;
                }

                if (command.TCS.Task.IsCanceled) {
                    continue;
                }
                try {
                    Stream.Position = command.Offset;
                    Stream.Read(command.Data, 0, command.Data.Length);
                    command.TCS.TrySetResult(true);
                }
                catch (Exception e) {
                    command.TCS.TrySetException(new InternalIOException("An exception occured while reading data", e));
                }
            }
        }

        public Task<bool> AddTask(byte[] data, long offset, CancellationToken ct) {
            if (WasDisposed) {
                throw new ObjectDisposedException("ReadController");
            }
            var command = new ReadCommand(offset, data);
            ct.Register(() => command.TCS.SetCanceled(ct));
            Inner.TryAdd(command, -1, ct);
            return command.TCS.Task;
        }

        public void Dispose() {
            WasDisposed = true;
            Inner?.Dispose();
        }        
    }
}
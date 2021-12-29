using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace LocalFS.Driver.Model {
    //TODO тут надо что-то городить на WaitHandle, но я не хочу :( 
    internal class FileHandlesLocksManager {
        private readonly ConcurrentDictionary<int, ClusterIndexLocks> locks = new();

        public IDisposable OpenRead(int index, long handle) {
            ClusterIndexLocks fhLock = locks.AddOrUpdate(
                index,
                ClusterIndexLocks.ConsRead(handle),
                (_, innerLock) => {
                    innerLock.Readers.Add(handle);
                    return innerLock;
                }
            );
            return new ReadLock(fhLock, handle);
        }

        public IDisposable OpenWrite(int index, long handle, CancellationToken ct) {
            ClusterIndexLocks result;
            SpinWait spinWait = new SpinWait();
            do {
                ct.ThrowIfCancellationRequested();
                result = locks.AddOrUpdate(
                    index,
                    ClusterIndexLocks.ConsWrite(handle),
                    (_, fhLock) => {
                        fhLock.Writer ??= handle;
                        return fhLock;
                    }
                );
                if (ct.IsCancellationRequested) {
                    if (result.Writer == handle) {
                        result.Writer = null;
                    }
                    ct.ThrowIfCancellationRequested();
                }
                spinWait.SpinOnce();
            } while (result.Writer != handle);
            return new WriteLock(result, handle);
        }

        public bool TryOpenWrite(int index, long handle, out IDisposable? writeLock) {
            var fhLock = locks.AddOrUpdate(
                index,
                ClusterIndexLocks.ConsWrite(handle),
                (_, inner) => {
                    inner.Writer ??= handle;
                    return inner;
                }
            );
            if (fhLock.Writer == handle) {
                writeLock = new WriteLock(fhLock, handle);
                return true;
            }
            else {
                writeLock = null;
                return false;
            }
        }
        
        private class ClusterIndexLocks {
            public readonly HashSet<long> Readers = new HashSet<long>();
            public long? Writer;

            public static ClusterIndexLocks ConsRead(long handle) {
                var result = new ClusterIndexLocks();
                result.Readers.Add(handle);
                return result;
            }

            public static ClusterIndexLocks ConsWrite(long handle) {
                var result = new ClusterIndexLocks();
                result.Writer = handle;
                return result;
            }
        }

        private class ReadLock : IDisposable {
            private readonly ClusterIndexLocks clusterLocks;
            private readonly long handleIndex;

            public ReadLock(ClusterIndexLocks clusterLocks, long handleIndex) {
                this.clusterLocks = clusterLocks;
                this.handleIndex = handleIndex;
            }

            public void Dispose() {
                if (!clusterLocks.Readers.Remove(handleIndex)) {
                    throw new InternalIOException($"Smth wrong with read locks for handle {handleIndex}", null);
                }
            }
        }

        private class WriteLock : IDisposable {
            private readonly ClusterIndexLocks clusterLocks;
            private readonly long handleIndex;

            public WriteLock(ClusterIndexLocks clusterLocks, long handleIndex) {
                this.clusterLocks = clusterLocks;
                this.handleIndex = handleIndex;
            }

            public void Dispose() {
                if (clusterLocks.Writer != handleIndex) {
                    throw new InternalIOException($"Smth wrong with write locks for handle {handleIndex}", null);
                }
                clusterLocks.Writer = null;
            }
        }
    }
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LocalFS.Driver.Model.ClustersAllocator {
    //TODO - может быть offheap, может быть MemoryMapped сделать, может тупо побить на чанки и грузить по мере использования и только при различиях
    //сейчас тут вообще неэффективно!!!!
    internal class ClustersAllocator {
        private ClustersBitArray ActiveBits { get; }
        private ClustersBitArray RealBits { get; }
        private ReaderWriterLockSlim RWLock { get; }
        private FileSystem FileSystem { get; }

        public ClustersAllocator(FileSystem fileSystem, int clustersSize) {
            ActiveBits = new ClustersBitArray(clustersSize);
            RealBits = new ClustersBitArray(clustersSize);
            RWLock = new ReaderWriterLockSlim();
            FileSystem = fileSystem;
        }

        public ClustersTransaction OpenTransaction() {
            return new ClustersTransaction(this);
        }

        public bool IsFreeCluster(int clusterIndex) {
            RWLock.EnterReadLock();
            try {
                return ActiveBits[clusterIndex];
            }
            finally {
                RWLock.ExitReadLock();
            }
        }

        public void Undo(List<ClusterAllocatorCommand> commands) {
            RWLock.EnterWriteLock();
            try {
                foreach (var command in commands) {
                    if (command.Allocate) {
                        ActiveBits[command.ClusterIndex] = false;
                    }
                }
            }
            finally {
                RWLock.ExitWriteLock();
            }
        }

        public async Task Commit(List<ClusterAllocatorCommand> commands) {
            // Да, тут я перемешиваю асинхронное и синхронное программирование,
            // но не хочу тащить какой-нибудь VisualStudio.Threading ради AsyncReaderWriterLock
            Task task = Task.Run(() => {
                RWLock.EnterWriteLock();
                try {
                    foreach (var command in commands) {
                        RealBits[command.ClusterIndex] = command.Allocate;
                        ActiveBits[command.ClusterIndex] = command.Allocate;
                    }
                    FileSystem.WriteController.WriteNow(
                        RealBits.Bytes,
                        1L * FileSystem.FileSystemInfo.BitMaskStartClusterIndex * FileSystem.FileSystemInfo.ClusterSize
                    );
                }
                finally {
                    RWLock.ExitWriteLock();
                }
            });
            await task;
        }
    }
}
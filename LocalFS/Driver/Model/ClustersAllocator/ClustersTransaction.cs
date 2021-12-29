using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalFS.Driver.Model.ClustersAllocator {
    internal struct ClustersTransaction:IDisposable {
        private ClustersAllocator ClustersAllocator { get; }
        private List<ClusterAllocatorCommand> Commands { get; }
        private bool Disposed { get; set; }

        public ClustersTransaction(ClustersAllocator clustersAllocator) {
            ClustersAllocator = clustersAllocator;
            Commands = new List<ClusterAllocatorCommand>();
            Disposed = false;
        }

        public void Dispose() {
            if (Disposed) {
                throw new ObjectDisposedException(nameof(ClustersTransaction));
            }
            if (Commands.Count == 0) {
                return;
            }
            ClustersAllocator.Undo(Commands);
            Disposed = true;
        }

        public async Task Commit() {
            if (Disposed) {
                throw new ObjectDisposedException(nameof(ClustersTransaction));
            }
            await ClustersAllocator.Commit(Commands);
            Commands.Clear();
        }
    }
}
namespace LocalFS.Driver.Model.ClustersAllocator {
    internal struct ClusterAllocatorCommand {
        public int ClusterIndex { get; }
        public bool Allocate { get; }

        public ClusterAllocatorCommand(int clusterIndex, bool allocate) {
            ClusterIndex = clusterIndex;
            Allocate = allocate;
        }
    }
}
namespace LocalFS.Driver.Model {
    public class FileSystemInfo {
        public int Version { get; }
        public int ClusterSize { get; }
        public long VolumeSize { get; }
        public int DataStartClusterIndex { get; }
        public int BitMaskStartClusterIndex => 1;
        public int EntriesStartClusterIndex { get; }
    }
}
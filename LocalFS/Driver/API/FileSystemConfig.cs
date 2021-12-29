using System;

namespace LocalFS.Driver.API {
    public class FileSystemConfig {
        public int Version { get; }
        public long VolumeSize { get; }
        public int ClusterSize { get; }

        private FileSystemConfig(int version, long volumeSize, int clusterSize) {
            Version = version;
            VolumeSize = volumeSize;
            ClusterSize = clusterSize;
        }

        public class Builder {
            public int Version { get; private set; }
            public long VolumeSize { get; private set; }
            public int ClusterSize { get; private set; }

            public static Builder New(long volumeSize) {
                return new Builder(1, 4 * 1024).WithVolumeSize(volumeSize);
            }

            private Builder(int version, int clusterSize) {
                Version = version;
                ClusterSize = clusterSize;
            }

            public Builder WithVolumeSize(long volumeSize) {
                if (volumeSize <= 0) {
                    throw new ArgumentException("Volume size should be great of zero", nameof(volumeSize));
                }
                VolumeSize = volumeSize;
                return this;
            }

            public Builder WithClusterSize(int clusterSize) {
                if (clusterSize <= 0) {
                    throw new ArgumentException("Volume size should be great of zero", nameof(clusterSize));
                }
                ClusterSize = clusterSize;
                return this;
            }

            public FileSystemConfig Build() {
                return new FileSystemConfig(
                    Version,
                    VolumeSize,
                    ClusterSize
                );
            }
        }
    }
}
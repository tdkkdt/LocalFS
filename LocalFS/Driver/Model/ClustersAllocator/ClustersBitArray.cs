using System;

namespace LocalFS.Driver.Model.ClustersAllocator {
    internal class ClustersBitArray {
        public int Size { get; }
        public byte[] Bytes { get; }

        public ClustersBitArray(int clustersSize) {
            Size = clustersSize;
            Bytes = new byte[(clustersSize + 7) / 8];
        }

        public bool Get(int clusterIndex) {
            if (clusterIndex >= Size) {
                throw new ArgumentOutOfRangeException(nameof(clusterIndex));
            }
            int byteIndex = clusterIndex >> 3;
            int inByteIndex = 7 - (clusterIndex & 7);
            return (Bytes[byteIndex] & (1 << inByteIndex)) != 0;
        }

        public void Set(int clusterIndex, bool value) {
            if (clusterIndex >= Size) {
                throw new ArgumentOutOfRangeException(nameof(clusterIndex));
            }
            int byteIndex = clusterIndex >> 3;
            int inByteIndex = 7 - (clusterIndex & 7);
            int mask = 1 << inByteIndex;
            if (value) {
                Bytes[byteIndex] = (byte)(Bytes[byteIndex] | mask);
            }
            else {
                Bytes[byteIndex] = (byte)(Bytes[byteIndex] & ~mask);
            }
        }

        public bool this[int clusterIndex] {
            get => Get(clusterIndex);
            set => Set(clusterIndex, value);
        }
    }
}
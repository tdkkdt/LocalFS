using System;

namespace LocalFS {
    public interface FileInfo {
        string Name { get; }
        string Path { get; }
        long Size { get; }
        DateTime CreationTime { get; }
        DateTime ModificationTime { get; }
    }
}
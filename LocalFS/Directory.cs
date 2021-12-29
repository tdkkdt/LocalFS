using System.Collections.Generic;

namespace LocalFS {
    public interface Directory {
        void Delete(string path);
        bool Exists(string path);
        void Move(string src, string dst);
        DirectoryInfo CreateDirectory(string path);

        IEnumerable<DirectoryInfo> EnumerateDirectories(string path) {
            return EnumerateDirectories(path, ".*");
        }

        IEnumerable<DirectoryInfo> EnumerateDirectories(string path, string pattern);

        IEnumerable<FileInfo> EnumerateFiles(string path) {
            return EnumerateFiles(path, ".*");
        }

        IEnumerable<FileInfo> EnumerateFiles(string path, string pattern);
        DirectoryInfo[] GetDirectories(string path, string pattern);

        DirectoryInfo[] GetDirectories(string path) {
            return GetDirectories(path, ".*");
        }

        // Directory.GetFiles(path, pattern);
        // Directory.GetParent(path);
        // Directory.GetCreationTime(path);
        // Directory.GetModificationTime(path);
        
    }
}
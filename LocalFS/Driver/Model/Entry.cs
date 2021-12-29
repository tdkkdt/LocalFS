using System;

namespace LocalFS.Driver.Model {
    class Entry {
        public const long MAXIMUM_LENGTH = 8 + 4 + 4 * 255 + 8 + 8; //Index + name Length + name + creationTime + modificationTime  
        public int Index { get; }
        public string Name { get; }
        public EntryType Type { get; }
        public DateTime CreationTime { get; }
        public DateTime ModificationTime { get; }
        public Data.Data Data { get; }

        public Entry(int index, string name, EntryType type, DateTime creationTime, DateTime modificationTime, Data.Data data) {
            Index = index;
            Name = name;
            Type = type;
            CreationTime = creationTime;
            ModificationTime = modificationTime;
            Data = data;
        }
    }
}
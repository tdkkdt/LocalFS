using System;
using System.IO;

namespace LocalFS.Driver.Model.Data {
    internal static class DataManager {
        internal static Data ReadData(FileSystem fileSystem, BinaryReader binaryReader) {
            DataType dataType = (DataType)binaryReader.ReadInt32();
            return dataType switch {
                DataType.RESIDENT => ResidentData.Read(fileSystem, binaryReader),
                DataType.COMPACT => CompactData.Read(fileSystem, binaryReader),
                DataType.EXTENDED => ExtendedData.Read(fileSystem, binaryReader),
                _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType.ToString())
            };
        }

        public static void WriteData(Data data, BinaryWriter binaryWriter) {
            switch (data) {
                case ResidentData:
                    binaryWriter.Write((int)DataType.RESIDENT);
                    break;
                case CompactData:
                    binaryWriter.Write((int)DataType.COMPACT);
                    break;
                case ExtendedData:
                    binaryWriter.Write((int)DataType.EXTENDED);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(data.GetType().ToString());
            }
            data.Write(binaryWriter);
        }
    }
}
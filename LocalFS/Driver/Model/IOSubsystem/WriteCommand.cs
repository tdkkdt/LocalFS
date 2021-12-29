using System.Threading.Tasks;

namespace LocalFS.Driver.Model.IOSubsystem {
    public class WriteCommand {
        //TODO в структуру
        public long Offset { get; }
        public byte[] Data { get; } //TODO в Memory/Span
        public TaskCompletionSource<bool> TCS { get; }

        public WriteCommand(long offset, byte[] data) {
            Offset = offset;
            Data = data;
            TCS = new TaskCompletionSource<bool>();
        }
    }
}
using System;

namespace LocalFS.Driver.Model {
    public class InternalIOException : Exception {
        public InternalIOException(string? message, Exception? innerException) : base(message, innerException) {
        }
    }
}
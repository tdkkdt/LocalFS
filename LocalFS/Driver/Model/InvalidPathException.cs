using System;

namespace LocalFS.Driver.Model {
    public class InvalidPathException : Exception {
        public InvalidPathException(string? message) : base(message) {
        }
    }
}
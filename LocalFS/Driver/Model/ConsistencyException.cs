using System;

namespace LocalFS.Driver.Model {
    public class ConsistencyException : Exception {
        public ConsistencyException(string? message) : base(message) {
        }
    }
}
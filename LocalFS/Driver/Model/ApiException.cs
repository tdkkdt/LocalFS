using System;

namespace LocalFS.Driver.Model {
    public class ApiException : Exception {
        public ApiException(string? message) : base(message) {
        }
    }
}
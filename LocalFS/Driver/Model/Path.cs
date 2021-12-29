using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalFS.Driver.Model {
    public class Path {
        private const char SEPARATOR = '/';
        internal string[] Tokens { get; }

        public static Path From(string absolutePath) {
            if (!absolutePath.StartsWith(SEPARATOR)) {
                throw new InvalidPathException("Absolute path should starts from / but " + absolutePath);
            }
            if (absolutePath.Any(c => c is '*' or '?')) {
                throw new InvalidPathException("Absolute path shouldn't contains '*' and '?' but " + absolutePath);
            }
            var tokens = absolutePath.Split(SEPARATOR);
            if (tokens.Any(string.IsNullOrEmpty)) {
                throw new InvalidPathException("Absolute path shouldn't contains empty parts but " + absolutePath);
            }
            return new Path(tokens);
        }

        private Path(string[] tokens) {
            Tokens = tokens;
        }

        public override string ToString() {
            return SEPARATOR + string.Join(SEPARATOR, Tokens);
        }
    }
}
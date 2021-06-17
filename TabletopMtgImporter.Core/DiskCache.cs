using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter
{
    public sealed class DiskCache : ICache
    {
        private static readonly string Cache = Path.Combine(Path.GetTempPath(), "TabletopMtgImporter", "Cache");

        public Task<string?> GetValueOrDefaultAsync(string key)
        {
            var path = GetCachePath(key);
            return Task.FromResult(File.Exists(path) ? File.ReadAllText(path) : null);
        }

        public Task SetValueAsync(string key, string value)
        {
            var path = GetCachePath(key);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, value);
            return Task.CompletedTask;
        }

        private static string GetCachePath(string key) => Path.Combine(Cache, HashKey(key));

        private static string HashKey(string key)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(key);
            var hash = sha.ComputeHash(bytes);
            return new BigInteger(hash).ToString("x");
        }
    }
}

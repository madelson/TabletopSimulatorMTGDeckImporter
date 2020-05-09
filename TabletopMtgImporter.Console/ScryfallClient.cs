using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter
{
    internal class ScryfallClient
    {
        private static readonly string Cache = Path.Combine(Path.GetTempPath(), "TabletopMtgImporter", "Cache");

        private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://api.scryfall.com/") };

        public async Task<T> GetJsonAsync<T>(string url)
        {
            if (this.TryGetCachedResponse<T>(url, out var cached))
            {
                return cached;
            }

            System.Console.WriteLine($"Downloading {url}");

            // rate-limiting requested by scryfall
            await Task.Delay(TimeSpan.FromMilliseconds(50));

            using (var response = await this._httpClient.GetAsync(url))
            {
                if (!response.IsSuccessStatusCode)
                {
                    var body = await (response.Content?.ReadAsStringAsync() ?? Task.FromResult("n/a"));
                    throw new InvalidOperationException($"Request to {url} failed with status code {response.StatusCode}. Body: '{body}'");
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(content); // ensure deserializes before caching
                this.AddToCache(url, content);
                return result;
            }
        }

        private bool TryGetCachedResponse<T>(string url, out T value)
        {
            var path = GetCachePath(url);
            if (File.Exists(path))
            {
                try
                {
                    value = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
                    return true;
                }
                catch (Exception ex)
                {
                    System.Console.Error.WriteLine($"Possible cache corruption: {path}: {ex}");
                }
            }

            value = default;
            return false;
        }

        private void AddToCache(string url, string content)
        {
            var path = GetCachePath(url);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, content);
        }

        private static string GetCachePath(string url) => Path.Combine(Cache, HashUrl(url));

        private static string HashUrl(string url)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(url);
                var hash = sha.ComputeHash(bytes);
                return new BigInteger(hash).ToString("x");
            }
        }
    }
}

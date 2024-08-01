using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace TabletopMtgImporter
{
    internal class ScryfallClient
    {
        private static readonly HttpClient HttpClient = new() 
        { 
            BaseAddress = new Uri("https://api.scryfall.com/"),
            // required by Scryfall now: https://scryfall.com/docs/api
            DefaultRequestHeaders =
            {
                Accept = { new("application/json") },
                UserAgent = { new("TabletopSimulatorMtgImporter", Assembly.GetEntryAssembly().GetName().Version.ToString()) }
            }
        };

        private readonly ILogger _logger;
        private readonly ICache _cache;

        public ScryfallClient(ILogger logger, ICache cache)
        {
            this._logger = logger;
            this._cache = cache;
        }

        public async Task<T> GetJsonAsync<T>(string url)
            where T : class
        {
            var cachedResponse = await this.GetCachedResponseOrDefaultAsync<T>(url).ConfigureAwait(false);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            this._logger.Info($"Downloading {url}");

            // rate-limiting requested by scryfall
            await Task.Delay(TimeSpan.FromMilliseconds(50)).ConfigureAwait(false);

            using var response = await HttpClient.GetAsync(url).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var body = await (response.Content?.ReadAsStringAsync() ?? Task.FromResult("n/a")).ConfigureAwait(false);
                throw new InvalidOperationException($"Request to {url} failed with status code {response.StatusCode}. Body: '{body}'");
            }

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = JsonConvert.DeserializeObject<T>(content)!; // ensure deserializes before caching
            await this._cache.SetValueAsync(url, content).ConfigureAwait(false);
            return result;
        }

        private async Task<T?> GetCachedResponseOrDefaultAsync<T>(string url)
            where T : class
        {
            var cachedValue = await this._cache.GetValueOrDefaultAsync(url).ConfigureAwait(false);
            if (cachedValue != null)
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(cachedValue);
                }
                catch (Exception ex)
                {
                    this._logger.Debug($"Possible cache corruption: key={url}: {ex}");
                }
            }

            return null;
        }
    }
}

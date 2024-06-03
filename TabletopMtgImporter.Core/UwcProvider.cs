using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter
{
    internal static class UwcProvider
    {
        private const string BaseUrl = "https://madelson.github.io/universes-within-collection/cards";

        private static readonly Lazy<Task<IReadOnlyDictionary<string, CardInfo>>> CardNames = new(DownloadUwcCards);

        public static async Task UpdateAsync(ScryfallCard card, IDeckInput input)
        {
            if (!input.UwcSetRegex.IsMatch(card.Set)
                || !(await CardNames.Value).TryGetValue(card.Name, out var cardInfo)) 
            { 
                return;
            }

            if (TabletopDeckCreator.IsDoubleFaced(card))
            {
                foreach (var face in card.Faces!)
                {
                    face.ImageUris["large"] = new($"{BaseUrl}/{UrlEncodeCardName(face.Name)}.png");
                }
            }
            else
            {
                card.ImageUris!["large"] = new($"{BaseUrl}/{UrlEncodeCardName(card.Name)}.png");
            }

            if (cardInfo.Nickname != null)
            {
                card.Name = cardInfo.Nickname;
            }
        }

        private static async Task<IReadOnlyDictionary<string, CardInfo>> DownloadUwcCards()
        {
            using HttpClient client = new();
            using var cardsResponse = await client.GetAsync($"{BaseUrl}/cards.json");
            cardsResponse.EnsureSuccessStatusCode();
            var cards = JsonConvert.DeserializeObject<Dictionary<string, CardInfo>>(await cardsResponse.Content.ReadAsStringAsync());
            return cards!.Values.ToDictionary(c => c.Name);
        }

        // github pages hosts the urls with %20 instead of + for spaces.
        private static string UrlEncodeCardName(string name) => WebUtility.UrlEncode(name).Replace("+", "%20");

        private class CardInfo
        {
            [JsonProperty(Required = Required.Always)]
            public string Name { get; set; } = default!;
            public string? Nickname { get; set; } = default!;
        }
    }
}

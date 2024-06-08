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
        private static readonly Lazy<Task<IReadOnlyDictionary<Guid, CardInfo>>> CardsByOracleId = new(DownloadUwcCards);

        public static async Task UpdateAsync(ScryfallCard card, IDeckInput input)
        {
            if (!input.UwcSetRegex.IsMatch(card.Set)
                // If the card is reversible, just use the ID from the first face. Possibly we should be treating each face separately,
                // but let's cross that bridge when we come to it if someone really wants to use the reversible cards.
                || !(await CardsByOracleId.Value).TryGetValue(card.OracleId ?? card.Faces![0].OracleId!.Value, out var cardInfo)) 
            { 
                return;
            }

            if (TabletopDeckCreator.IsDoubleFaced(card))
            {
                card.Faces![0].ImageUris["large"] = cardInfo.Image;
                card.Faces![1].ImageUris["large"] = cardInfo.BackImage ?? throw new InvalidOperationException($"Missing back image for {card.Name} ({card.OracleId})");
            }
            else
            {
                card.ImageUris!["large"] = cardInfo.Image;
            }

            if (cardInfo.Nickname != null)
            {
                card.Name = cardInfo.Nickname;
            }
        }

        private static async Task<IReadOnlyDictionary<Guid, CardInfo>> DownloadUwcCards()
        {
            using HttpClient client = new();
            using var cardsResponse = await client.GetAsync($"https://madelson.github.io/universes-within-collection/gallery/cardData.json");
            cardsResponse.EnsureSuccessStatusCode();
            var cards = JsonConvert.DeserializeObject<CardInfo[]>(await cardsResponse.Content.ReadAsStringAsync());
            return cards.ToDictionary(c => c.OracleId);
        }

        private class CardInfo
        {
            [JsonProperty(Required = Required.Always)]
            public Guid OracleId { get; set; }
            [JsonProperty(Required = Required.Always)]
            public string Name { get; set; } = default!;
            public string? Nickname { get; set; }
            [JsonProperty(Required = Required.Always)]
            public Uri Image { get; set; } = default!;
            public Uri? BackImage { get; set; } = default!;
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter
{
    // based on https://scryfall.com/docs/api/cards
    internal class ScryfallCard
    {
        public string Name = default!;
        public string Set = default!;
        /// <summary>
        /// See <see cref="DeckCard.CollectorNumber"/> for why this is a <see cref="string"/> rather than an <see cref="int"/>
        /// </summary>
        [JsonProperty("collector_number")]
        public string CollectorNumber = default!;
        // see https://scryfall.com/docs/api/layouts
        public string? Layout;

        [JsonProperty("card_faces")]
        public Face[]? Faces;
        [JsonProperty("image_uris")]
        public Dictionary<string, Uri>? ImageUris;
        [JsonProperty("all_parts")]
        public RelatedCard[]? RelatedCards;

        public class Face
        {
            [JsonProperty("image_uris")]
            public Dictionary<string, Uri> ImageUris = default!;
        }

        public class RelatedCard
        {
            public string Name = default!;
            public Uri Uri = default!;
            public string Component = default!;
        }
    }
}

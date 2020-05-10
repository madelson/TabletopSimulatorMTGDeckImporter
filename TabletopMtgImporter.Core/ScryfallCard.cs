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
        [JsonProperty("collector_number")]
        public int CollectorNumber;
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
        }
    }
}

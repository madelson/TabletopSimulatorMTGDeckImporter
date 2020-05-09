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
        public string Lang;
        public string Name;
        // see https://scryfall.com/docs/api/layouts
        public string Layout;

        [JsonProperty("card_faces")]
        public Face[] Faces;
        [JsonProperty("image_uris")]
        public Dictionary<string, Uri> ImageUris;
        [JsonProperty("all_parts")]
        public RelatedCard[] RelatedCards;

        public class Face
        {
            [JsonProperty("image_uris")]
            public Dictionary<string, Uri> ImageUris;
        }

        public class RelatedCard
        {
            public string Name;
            public Uri Uri;
        }
    }
}

using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TabletopMtgImporter.Tests.Scripts
{
    public class RoboRosewaterDownloader
    {
        [Test]
        public void DownloadRoboRosewaterCube()
        {
            var html = new WebClient().DownloadString("http://www.planesculptors.net/set/roborosewater-cube/version-1");
            var start = html.IndexOf("var cardData = ") + "var cardData = ".Length;
            var end = html.IndexOf("];", start) - 1;
            var cardsJson = html.Substring(start, end - start);
            var cardImageUrls = Regex.Matches(cardsJson, "artUrl: \"(?<url>.*?)\"")
                .Cast<Match>()
                .Select(m => m.Groups["url"].Value);

            var cards = cardImageUrls.Select(u => new { url = u, name = WebUtility.UrlDecode(Path.GetFileNameWithoutExtension(u)) })
                .ToArray();

            var cardsAndRelatedCards = cards.ToDictionary(
                c => new DeckCard(c.name, set: null, collectorNumber: null, isCommander: false),
                c => new ScryfallCard
                {
                    Name = c.name,
                    ImageUris = new Dictionary<string, Uri> { ["large"] = new Uri(c.url) }
                }
            );

            var deck = TabletopDeckCreator.CreateDeck(cardsAndRelatedCards.Keys.OrderBy(c => c.Name).ToArray(), cardsAndRelatedCards);
            var json = JsonConvert.SerializeObject(deck, Formatting.Indented);
            File.WriteAllText(Path.Combine(new Configuration().OutputDirectory, "RoboRosewater.json"), json);
        }
    }
}

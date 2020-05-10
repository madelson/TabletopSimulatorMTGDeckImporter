using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace TabletopMtgImporter.Console
{
    using Console = System.Console;

    internal static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            if (Debugger.IsAttached && args.Length == 0)
            {
                args = new[] { @"C:\Users\mikea_000\Downloads\Sidar and Ikra.txt" };
            }

            if (args.Contains("--debug"))
            {
                args = args.Where(a => a != "--debug").ToArray();
                Debugger.Launch();
            }

            if (args.Length != 1)
            {
                Console.Error.WriteLine($"Usage {typeof(Program).Assembly.GetName().Name} <deckFile>");
                return 1;
            }

            var deckFile = args[0];
            if (!File.Exists(deckFile))
            {
                Console.Error.WriteLine($"File '{deckFile}' does not exist");
                return 2;
            }

            List<DeckCard> cards;
            using (var reader = new StreamReader(deckFile))
            {
                if (!DeckParser.TryParse(reader, out cards, out var errorLines))
                {
                    Console.Error.WriteLine($"The following lines of {deckFile} could not be parsed. Make sure each line follows the format '<count> <cardname>':");
                    foreach (var errorLine in errorLines)
                    {
                        const int MaxPrintLength = 60;
                        const string Ellipsis = "...";
                        Console.Error.WriteLine($"\t{(errorLine.Length <= MaxPrintLength ? errorLine : errorLine.Substring(0, MaxPrintLength - Ellipsis.Length) + "...")}");
                    }
                    return 3;
                }
            }

            if (cards.Count != 100)
            {
                Console.Error.WriteLine($"WARNING: deck file contains {cards.Count} cards");
            }

            var cardInfo = new Dictionary<DeckCard, ScryfallCard>();
            var scryfallClient = new ScryfallClient();
            foreach (var card in cards.Distinct())
            {
                var url = card.CollectorNumber.HasValue
                    ? $"/cards/{WebUtility.UrlEncode(card.Set)}/{card.CollectorNumber}"
                    : $"/cards/named?exact={WebUtility.UrlEncode(card.Name)}{(card.Set != null ? $"&set={WebUtility.UrlEncode(card.Set)}" : string.Empty)}";

                var info = await scryfallClient.GetJsonAsync<ScryfallCard>(url).ConfigureAwait(false);
                
                cardInfo[card] = info;
                foreach (var relatedCard in info.RelatedCards ?? Enumerable.Empty<ScryfallCard.RelatedCard>())
                {
                    var relatedInfo = await scryfallClient.GetJsonAsync<ScryfallCard>(relatedCard.Uri.AbsoluteUri).ConfigureAwait(false);
                    cardInfo[new DeckCard(relatedInfo.Name, set: relatedInfo.Set, collectorNumber: relatedInfo.CollectorNumber, isCommander: false)] = relatedInfo;
                }
            }

            var deck = TabletopDeckCreator.CreateDeck(cards, cardInfo);
            var deckJson = JsonConvert.SerializeObject(deck, Formatting.Indented);
            var outputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Tabletop Simulator", "Saves", "Saved Objects");
            var outputPath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(deckFile) + ".json");
            File.WriteAllText(outputPath, deckJson);
            Console.WriteLine("Wrote output to " + outputPath);

            return 0;
        }
    }
}

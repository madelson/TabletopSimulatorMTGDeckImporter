using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter
{
    public sealed class Importer
    {
        private readonly ILogger _logger;
        private readonly Configuration _configuration;

        public Importer(ILogger logger, Configuration configuration)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public Task<bool> TryImportAsync(IDeckInput deckInput)
        {
            if (deckInput == null) { throw new ArgumentNullException(nameof(deckInput)); }
            return TryImportAsync();

            async Task<bool> TryImportAsync()
            {
                List<DeckCard> cards;
                using (var reader = deckInput.OpenReader())
                {
                    if (!DeckParser.TryParse(reader, out cards, out var errorLines))
                    {
                        var errorBuilder = new StringBuilder();
                        errorBuilder.AppendLine($"Some lines could not be parsed. Make sure each line follows one of the supported formats.")
                            .AppendLine("* 1 CardName format: '<count> <card name>'")
                            .AppendLine("* 1x CardName with metadata format: <count>x <card name> [(<set>)[ <collector number>]] [`<category>`]")
                            .AppendLine("Invalid lines:");
                        foreach (var errorLine in errorLines)
                        {
                            const int MaxPrintLength = 60;
                            const string Ellipsis = "...";
                            errorBuilder.AppendLine($"\t{(errorLine.Length <= MaxPrintLength ? errorLine : errorLine.Substring(0, MaxPrintLength - Ellipsis.Length) + "...")}");
                        }
                        this._logger.Error(errorBuilder.ToString());
                        return false;
                    }
                }

                if (cards.Count != 100)
                {
                    this._logger.Warning($"WARNING: deck file contains {cards.Count} cards");
                }

                var cardInfo = new Dictionary<DeckCard, ScryfallCard>();
                // This set allows us to avoid adding the same related card twice. We check based on Uri rather than name
                // because tokens can have the same name but different identity (e. g. Soldier with lifelink vs. not)
                var loadedRelatedCardUris = new HashSet<Uri>();
                var scryfallClient = new ScryfallClient(this._logger);
                var hasDownloadError = false;
                foreach (var card in cards.Distinct())
                {
                    var url = card.CollectorNumber != null
                        ? $"/cards/{WebUtility.UrlEncode(card.Set)}/{card.CollectorNumber}"
                        : $"/cards/named?exact={WebUtility.UrlEncode(card.Name)}{(card.Set != null ? $"&set={WebUtility.UrlEncode(card.Set)}" : string.Empty)}";

                    ScryfallCard? info;
                    try { info = await scryfallClient.GetJsonAsync<ScryfallCard>(url).ConfigureAwait(false); }
                    catch (Exception ex)
                    {
                        hasDownloadError = true;
                        info = null;
                        this._logger.Error($"Failed to download card '{card.Name}'{(card.Set != null ? $" ({card.Set})" : string.Empty)}{(card.CollectorNumber != null ? $" #{card.CollectorNumber}" : string.Empty)}");
                        this._logger.Debug($"Failed to download {url}. Detailed exception information: " + ex);
                    }

                    if (info != null)
                    {
                        cardInfo[card] = info;
                        foreach (var relatedCard in (info.RelatedCards ?? Enumerable.Empty<ScryfallCard.RelatedCard>())
                            .Where(rc => rc.Component != "combo_piece"))
                        {
                            if (loadedRelatedCardUris.Add(relatedCard.Uri))
                            {
                                try
                                {
                                    var relatedInfo = await scryfallClient.GetJsonAsync<ScryfallCard>(relatedCard.Uri.AbsoluteUri).ConfigureAwait(false);
                                    cardInfo[new DeckCard(relatedInfo.Name, set: relatedInfo.Set, collectorNumber: relatedInfo.CollectorNumber, isCommander: false)] = relatedInfo;
                                }
                                catch (Exception ex)
                                {
                                    this._logger.Warning($"Failed to download card '{relatedCard.Name}' related to '{info.Name}'");
                                    this._logger.Debug($"Failed to download related card {relatedCard.Uri}. Detailed exception information: " + ex);
                                }
                            }
                        }
                    }
                }

                if (hasDownloadError) { return false; }

                var deck = TabletopDeckCreator.CreateDeck(cards, cardInfo);
                var deckJson = JsonConvert.SerializeObject(deck, Formatting.Indented);
                Directory.CreateDirectory(this._configuration.OutputDirectory); // ensure created
                var outputPath = Path.Combine(this._configuration.OutputDirectory, Path.GetFileNameWithoutExtension(deckInput.Name) + ".json");
                File.WriteAllText(outputPath, deckJson);
                this._logger.Info("Wrote output to " + outputPath);

                return true;
            }
        }
    }
}

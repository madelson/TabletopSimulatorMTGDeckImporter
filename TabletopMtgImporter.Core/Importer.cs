using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TabletopMtgImporter
{
    public sealed class Importer
    {
        /// <summary>
        /// Based on MTG comprehensive rules with "emblem" and "card" added
        /// </summary>
        private static readonly Regex NonStandardCardTypesRegex = 
            new(@"\b(conspiracy|dungeon|phenomenon|plane|scheme|vanguard|emblem|card)\b", RegexOptions.IgnoreCase);

        private readonly ILogger _logger;
        private readonly ICache _cache;
        private readonly ISaver _saver;

        public Importer(ILogger logger, ICache cache, ISaver saver)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this._saver = saver ?? throw new ArgumentNullException(nameof(saver));
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
                            .AppendLine("* 1x CardName with metadata format: '<count>x <card name> [(<set>)[ <collector number>]] [`<category>`]' (categories may alternatively be surrounded with square brackets [] instead of backticks ``)")
                            .AppendLine("Visit https://github.com/madelson/TabletopSimulatorMTGDeckImporter/blob/master/docs/instructions.md for more information.")
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
                    this._logger.Warning($"WARNING: deck contains {cards.Count} card{(cards.Count == 1 ? string.Empty : "s")}");
                }

                var cardInfo = new Dictionary<DeckCard, ScryfallCard>();
                // This set allows us to avoid adding the same related card twice. We check based on Uri rather than name
                // because tokens can have the same name but different identity (e. g. Soldier with lifelink vs. not)
                var loadedRelatedCardUris = new HashSet<Uri>();
                var scryfallClient = new ScryfallClient(this._logger, this._cache);
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
                        await UwcProvider.UpdateAsync(info, deckInput);

                        cardInfo[card] = info;
                        foreach (var relatedCard in (info.RelatedCards ?? Enumerable.Empty<ScryfallCard.RelatedCard>())
                            .Where(rc => rc.Component != "combo_piece" || NonStandardCardTypesRegex.IsMatch(rc.TypeLine)))
                        {
                            if (loadedRelatedCardUris.Add(relatedCard.Uri))
                            {
                                try
                                {
                                    var relatedInfo = await scryfallClient.GetJsonAsync<ScryfallCard>(relatedCard.Uri.AbsoluteUri).ConfigureAwait(false);
                                    await UwcProvider.UpdateAsync(relatedInfo, deckInput);
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
                await this._saver.SaveAsync(name: Path.GetFileNameWithoutExtension(deckInput.Name) + ".json", contents: deckJson);

                return true;
            }
        }
    }
}

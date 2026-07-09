using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter
{
    internal static class TabletopDeckCreator
    {
        public static TabletopDeckObject CreateDeck(IReadOnlyList<DeckCard> cards, IReadOnlyDictionary<DeckCard, ScryfallCard> cardsAndRelatedCards)
        {
            var mainDeckCards = cards.OrderByDescending(c => c.IsCommander)
                .ToArray();
            var tokens = cardsAndRelatedCards.Keys.Except(cards)
                .ToArray();
            var doubleFacedCards = cardsAndRelatedCards.Values.Where(IsDoubleFaced)
                .ToArray();

            var deck = new TabletopDeckObject
            {
                ObjectStates = new List<TabletopDeckObject.ObjectState>
                {
                    // main deck
                    new TabletopDeckObject.ObjectState
                    {
                        Name = "DeckCustom",
                        ContainedObjects = mainDeckCards.Select((c, index) => new TabletopDeckObject.CardReference
                            {
                                CardId = ToId(index),
                                Name = "Card",
                                // Use name from info to get UWC name if available
                                Nickname = cardsAndRelatedCards[c].Name,
                            })
                            .ToList(),
                        DeckIds = Enumerable.Range(0, mainDeckCards.Length).Select(ToId).ToList(),
                        CustomDeck = mainDeckCards.Select((c, index) => new { card = c, index })
                            .ToDictionary(
                                t => t.index + 1,
                                t => new TabletopDeckObject.CardInfo
                                {
                                    FaceUrl = RewriteUrl((cardsAndRelatedCards[t.card].ImageUris ?? cardsAndRelatedCards[t.card].Faces![0].ImageUris)["large"]),
                                }
                            ),
                        Transform = { PosY = 1 }
                    },

                    // tokens
                    new TabletopDeckObject.ObjectState
                    {
                        Name = "DeckCustom",
                        ContainedObjects = tokens.Select((c, index) => new TabletopDeckObject.CardReference
                            {
                                CardId = ToId(index),
                                Name = "Card",
                                // Use name from info to get UWC name if available
                                Nickname = cardsAndRelatedCards[c].Name,
                            })
                            .ToList(),
                        DeckIds = Enumerable.Range(0, tokens.Length).Select(ToId).ToList(),
                        CustomDeck = tokens.Select((c, index) => new { card = c, index })
                            .ToDictionary(
                                t => t.index + 1,
                                t => new TabletopDeckObject.CardInfo
                                {
                                    FaceUrl = RewriteUrl((cardsAndRelatedCards[t.card].ImageUris ?? cardsAndRelatedCards[t.card].Faces![0].ImageUris)["large"]),
                                    BackUrl = RewriteUrl(
                                        (cardsAndRelatedCards[t.card].ImageUris ?? cardsAndRelatedCards[t.card].Faces![1].ImageUris)["large"]
                                            ?? TabletopDeckObject.CardInfo.DefaultBackUrl)
                                }
                            ),
                        Transform = { PosX = 2.2, RotZ = 0 },
                    },

                    // double-face cards
                    new TabletopDeckObject.ObjectState
                    {
                        Name = "DeckCustom",
                        ContainedObjects = doubleFacedCards.Select((c, index) => new TabletopDeckObject.CardReference
                            {
                                CardId = ToId(index),
                                Name = "Card",
                                Nickname = c.Name,
                            })
                            .ToList(),
                        DeckIds = Enumerable.Range(0, doubleFacedCards.Length).Select(ToId).ToList(),
                        CustomDeck = doubleFacedCards.Select((c, index) => new { card = c, index })
                            .ToDictionary(
                                t => t.index + 1,
                                t => new TabletopDeckObject.CardInfo
                                {
                                    FaceUrl = RewriteUrl(t.card.Faces![0].ImageUris["large"]),
                                    BackUrl = RewriteUrl(t.card.Faces[1].ImageUris["large"]),
                                }
                            ),
                        Transform = { PosX = 2.2, RotZ = 0 },
                    },
                }
            };

            return deck;
        }

        static int ToId(int index) => 100 * (index + 1);

        public static bool IsDoubleFaced(ScryfallCard card) => card.Layout == "transform" || card.Layout == "modal_dfc";

        /// <summary>
        /// Scryfall has taken to returning URLs like https://cards.scryfall.io/large/front/1/d/1d52e527-3835-4350-8c01-0f2d5d623b9c.jpg?1782707107,
        /// which tabletop complains about because it can't determine the image type
        /// </summary>
        private static Uri RewriteUrl(Uri uri)
        {
            if (string.Equals(uri.Host, "cards.scryfall.io", StringComparison.OrdinalIgnoreCase))
            {
                var builder = new UriBuilder(uri)
                {
                    Host = "lucky-pond-4f97.mike-adelson314.workers.dev",
                };

                return builder.Uri;
            }

            return uri;
        }
    }
}

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
                                    FaceUrl = (cardsAndRelatedCards[t.card].ImageUris ?? cardsAndRelatedCards[t.card].Faces![0].ImageUris)["large"],
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
                                    FaceUrl = (cardsAndRelatedCards[t.card].ImageUris ?? cardsAndRelatedCards[t.card].Faces![0].ImageUris)["large"],
                                    BackUrl = (cardsAndRelatedCards[t.card].ImageUris ?? cardsAndRelatedCards[t.card].Faces![1].ImageUris)["large"]
                                        ?? TabletopDeckObject.CardInfo.DefaultBackUrl
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
                                    FaceUrl = t.card.Faces![0].ImageUris["large"],
                                    BackUrl = t.card.Faces[1].ImageUris["large"],
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
    }
}

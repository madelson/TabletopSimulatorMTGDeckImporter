using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter
{
    internal static class TabletopDeckCreator
    {
        public static TabletopDeckObject CreateDeck(IReadOnlyList<string> cards, IReadOnlyDictionary<string, ScryfallCard> cardsAndRelatedCards)
        {
            var tokens = cardsAndRelatedCards.Keys.Except(cards)
                .ToArray();
            var transformCards = cardsAndRelatedCards.Values.Where(c => c.Layout == "transform")
                .ToArray();

            var deck = new TabletopDeckObject
            {
                ObjectStates = new List<TabletopDeckObject.ObjectState>
                {
                    // main deck
                    new TabletopDeckObject.ObjectState
                    {
                        Name = "DeckCustom",
                        ContainedObjects = cards.Select((c, index) => new TabletopDeckObject.CardReference
                            {
                                CardId = ToId(index),
                                Name = "Card",
                                Nickname = c,
                            })
                            .ToList(),
                        DeckIds = Enumerable.Range(0, cards.Count).Select(ToId).ToList(),
                        CustomDeck = cards.Select((c, index) => new { card = c, index })
                            .ToDictionary(
                                t => t.index + 1,
                                t => new TabletopDeckObject.CardInfo
                                {
                                    FaceUrl = (cardsAndRelatedCards[t.card].ImageUris ?? cardsAndRelatedCards[t.card].Faces[0].ImageUris)["large"],
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
                                Nickname = c,
                            })
                            .ToList(),
                        DeckIds = Enumerable.Range(0, tokens.Length).Select(ToId).ToList(),
                        CustomDeck = tokens.Select((c, index) => new { card = c, index })
                            .ToDictionary(
                                t => t.index + 1,
                                t => new TabletopDeckObject.CardInfo
                                {
                                    FaceUrl = cardsAndRelatedCards[t.card].ImageUris["large"],
                                }
                            ),
                        Transform = { PosX = 2.2, RotZ = 0 },
                    },

                    // flip cards
                    new TabletopDeckObject.ObjectState
                    {
                        Name = "DeckCustom",
                        ContainedObjects = transformCards.Select((c, index) => new TabletopDeckObject.CardReference
                            {
                                CardId = ToId(index),
                                Name = "Card",
                                Nickname = c.Name,
                            })
                            .ToList(),
                        DeckIds = Enumerable.Range(0, transformCards.Length).Select(ToId).ToList(),
                        CustomDeck = transformCards.Select((c, index) => new { card = c, index })
                            .ToDictionary(
                                t => t.index + 1,
                                t => new TabletopDeckObject.CardInfo
                                {
                                    FaceUrl = t.card.Faces[0].ImageUris["large"],
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
    }
}

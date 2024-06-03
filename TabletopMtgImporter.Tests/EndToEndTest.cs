using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TabletopMtgImporter.Tests
{
    public class EndToEndTest
    {
        [TestCase("Archidekt1CardNameFormat.txt", ExpectedResult = "count=110,hash=uwIFHcN4qgAHynr8PUtidw==")]
        [TestCase("Archidekt1xCardNameCodeCategoryLabel.txt", ExpectedResult = "count=107,hash=8ihrLpfvLbshr11zjvJohw==")]
        [TestCase("MaybeboardAndAlternateArtCollectorNumber.txt", ExpectedResult = "count=113,hash=bPMsgich2klAw1wqpTnBlQ==")]
        [TestCase("ComboPieceRelatedCards.txt", ExpectedResult = "count=121,hash=17/Qnlk4ImPNXeJo03hFHA==")]
        [TestCase("Foils.txt", ExpectedResult = "count=112,hash=KuTaz4HCMEypbfOcQb1Idg==")]
        [TestCase("SameNameTokens.txt", ExpectedResult = "count=121,hash=17/Qnlk4ImPNXeJo03hFHA==")]
        [TestCase("ArchidektUpdatedCategoryFormat.txt", ExpectedResult = "count=101,hash=B+6eJzK8ihFObkI/htreeg==")]
        [TestCase("ArchidektUpdatedCategoryFormatMultipleCommanders.txt", ExpectedResult = "count=105,hash=adsSlvFJryKlNUh3mEUbqQ==")]
        [TestCase("DoubleSidedTokens.txt", ExpectedResult = "count=109,hash=OwA9GJN3VRVT+4GrLUN3pg==")]
        public async Task<string> TestRunsEndToEndWithoutErrors(string sampleName)
        {
            using var sample = SamplesHelper.GetSample(sampleName);

            var testLogger = new TestLogger();
            var importer = new Importer(testLogger, new DiskCache(), TestHelper.CreateSaver(testLogger));
            var result = await importer.TryImportAsync(new DeckFileInput(sample.Path, useUwcCards: true));
            Assert.IsEmpty(testLogger.ErrorLines);
            Assert.IsEmpty(testLogger.WarningLines);
            Assert.IsTrue(result);

            var outputFile = Path.Combine(TestHelper.OutputDirectory, Path.ChangeExtension(Path.GetFileName(sample.Path), ".json"));
            Assert.IsTrue(File.Exists(outputFile));

            var outputText = File.ReadAllText(outputFile);
            var parsed = JsonConvert.DeserializeObject<TabletopDeckObject>(outputText);
            var cardCount = parsed.ObjectStates.Sum(s => s.ContainedObjects.Count);
            var hashInput = string.Join(
                Environment.NewLine,
                parsed.ObjectStates.OrderBy(s => s.Name)
                    .SelectMany(s => s.ContainedObjects.Select(o => o.Nickname).OrderBy(s => s))
            );
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(hashInput));
            return $"count={cardCount},hash={Convert.ToBase64String(hash)}";
        }

        [TestCase("1x Chandra, Awakened Inferno")]
        [TestCase("1x Gideon of the Trials")]
        public async Task TestPlaneswalkerEmblems(string card)
        {
            var imported = await this.ImportDeckAsync(card);
            var cardName = imported.ObjectStates[0].ContainedObjects.Single().Nickname;
            CollectionAssert.AreEquivalent(new[] { cardName + " Emblem" }, imported.ObjectStates[1].ContainedObjects.Select(o => o.Nickname));
        }

        [TestCase("1x Ondu Inversion // Ondu Skyruins")]
        [TestCase("1x Branchloft Pathway // Boulderloft Pathway")]
        public async Task TestModalDoubleFacedCards(string card)
        {
            var imported = await this.ImportDeckAsync(card);
            var cardName = imported.ObjectStates[0].ContainedObjects.Single().Nickname;
            CollectionAssert.AreEquivalent(new[] { cardName }, imported.ObjectStates[2].ContainedObjects.Select(o => o.Nickname));
        }

        [Test]
        public async Task TestCommandSeparatedCategories()
        {
            var deck = await this.ImportDeckAsync("1x Terramorphic Expanse (dmc) [Maybeboard{noDeck}{noPrice},Land] ");
            Assert.IsEmpty(deck.ObjectStates[0].ContainedObjects); // all in maybeboard

            deck = await this.ImportDeckAsync("1x Terramorphic Expanse (dmc) [Land,Commander] ");
            Assert.AreEqual("Terramorphic Expanse", deck.ObjectStates[0].ContainedObjects.Single().Nickname);
        }

        [Test]
        public async Task TestCategoryOverridesMaybeboard()
        {
            using var sample = SamplesHelper.GetSample("BraidsCategoryOverridesMaybeboard.txt");
            var deck = await this.ImportDeckAsync(File.ReadLines(sample.Path), require100Cards: true);
            Assert.That(deck.ObjectStates[0].ContainedObjects.Select(o => o.Nickname), Does.Contain("Maro"));
        }

        [Test]
        public async Task TestUwc()
        {
            var cards = new[]
            {
                "1x Canoptek Wraith",
                "1x Barbara Wright (who) 14",
                "1x Atomize (pip) 94"
            };

            var deck = await this.ImportDeckAsync(cards);
            var mainDeck = deck.ObjectStates[0];
            CollectionAssert.AreEquivalent(
                new[] { "Elara, Tapestry Weaver", "Tangle Wraith", "Atomize" },
                mainDeck.ContainedObjects.Select(o => o.Nickname));
            CollectionAssert.AreEquivalent(
                new[]
                {
                    "https://madelson.github.io/universes-within-collection/cards/Canoptek%20Wraith.png",
                    "https://madelson.github.io/universes-within-collection/cards/Barbara%20Wright.png",
                    "https://madelson.github.io/universes-within-collection/cards/Atomize.png"
                },
                mainDeck.CustomDeck.Values.Select(c => c.FaceUrl.AbsoluteUri));
        }

        private Task<TabletopDeckObject> ImportDeckAsync(params string[] cards) => this.ImportDeckAsync(cards.AsEnumerable());

        private async Task<TabletopDeckObject> ImportDeckAsync(IEnumerable<string> cards, bool require100Cards = false)
        {
            var testLogger = new TestLogger();
            var importer = new Importer(testLogger, new DiskCache(), TestHelper.CreateSaver(testLogger));
            var deckInput = new StringDeckInput { Text = string.Join(Environment.NewLine, cards), UseUwcCards = true };
            Assert.IsTrue(await importer.TryImportAsync(deckInput), $"ERRORS: {string.Join(", ", testLogger.ErrorLines)}");
            Assert.IsEmpty(testLogger.ErrorLines);
            if (require100Cards) { Assert.IsEmpty(testLogger.WarningLines); }
            else 
            {
                Assert.AreEqual(1, testLogger.WarningLines.Count, message: string.Join(Environment.NewLine, testLogger.WarningLines));
                Assert.That(testLogger.WarningLines[0], Does.Match(@"^WARNING: deck contains \d+ card"));
            }
            var outputText = File.ReadAllText(Path.Combine(TestHelper.OutputDirectory, Path.GetFileNameWithoutExtension(deckInput.Name) + ".json"));
            return JsonConvert.DeserializeObject<TabletopDeckObject>(outputText);
        }

        private class StringDeckInput : IDeckInput
        {
            public string Text { get; set; }
            public bool UseUwcCards { get; set; }

            public string Name { get; } = $"TestDeck{Guid.NewGuid():n}";

            public Regex UwcSetRegex => new Regex(this.UseUwcCards ? "." : "$^", RegexOptions.IgnoreCase);

            public TextReader OpenReader() => new StringReader(this.Text);
        }
    }
}

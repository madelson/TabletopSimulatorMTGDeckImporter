using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter.Tests
{
    public class EndToEndTest
    {
        [TestCase("Archidekt1CardNameFormat.txt", ExpectedResult = "count=108,hash=1Tn1wG0GoooaY2MmHwvB2g==")]
        [TestCase("Archidekt1xCardNameCodeCategoryLabel.txt", ExpectedResult = "count=105,hash=6dg6IPECMxAOVUuKFcR8yQ==")]
        [TestCase("MaybeboardAndAlternateArtCollectorNumber.txt", ExpectedResult = "count=108,hash=zHg9fZWqNKk6wlJmoLBu0w==")]
        [TestCase("ComboPieceRelatedCards.txt", ExpectedResult = "count=115,hash=hthbJgpnr1Ul4CI3x8cB7A==")]
        [TestCase("Foils.txt", ExpectedResult = "count=109,hash=hSS2IV3TkyYBo3S1zHTUnw==")]
        [TestCase("SameNameTokens.txt", ExpectedResult = "count=115,hash=hthbJgpnr1Ul4CI3x8cB7A==")]
        [TestCase("ArchidektUpdatedCategoryFormat.txt", ExpectedResult = "count=101,hash=B+6eJzK8ihFObkI/htreeg==")]
        [TestCase("ArchidektUpdatedCategoryFormatMultipleCommanders.txt", ExpectedResult = "count=103,hash=jzAhbtVejcFgDWWJOzKFHw==")]
        public async Task<string> TestRunsEndToEndWithoutErrors(string sampleName)
        {
            using var sample = SamplesHelper.GetSample(sampleName);

            var testLogger = new TestLogger();
            var importer = new Importer(testLogger, new DiskCache(), TestHelper.CreateSaver(testLogger));
            var result = await importer.TryImportAsync(new DeckFileInput(sample.Path));
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

        private async Task<TabletopDeckObject> ImportDeckAsync(params string[] cards)
        {
            var testLogger = new TestLogger();
            var importer = new Importer(testLogger, new DiskCache(), TestHelper.CreateSaver(testLogger));
            var deckInput = new StringDeckInput { Text = string.Join(Environment.NewLine, cards) };
            Assert.IsTrue(await importer.TryImportAsync(deckInput));
            Assert.IsEmpty(testLogger.ErrorLines);
            Assert.AreEqual(1, testLogger.WarningLines.Count, message: string.Join(Environment.NewLine, testLogger.WarningLines));
            Assert.That(testLogger.WarningLines[0], Does.Match(@"^WARNING: deck contains \d+ card"));
            var outputText = File.ReadAllText(Path.Combine(TestHelper.OutputDirectory, Path.GetFileNameWithoutExtension(deckInput.Name) + ".json"));
            return JsonConvert.DeserializeObject<TabletopDeckObject>(outputText);
        }

        private class StringDeckInput : IDeckInput
        {
            public string Text { get; set; }

            public string Name { get; } = $"TestDeck{Guid.NewGuid():n}";

            public TextReader OpenReader() => new StringReader(this.Text);
        }
    }
}

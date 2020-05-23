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
        [TestCase("Archidekt1CardNameFormat.txt", ExpectedResult = "count=107,hash=7PEsdf5KnAN+LLwFv5dn1g==")]
        [TestCase("Archidekt1xCardNameCodeCategoryLabel.txt", ExpectedResult = "count=104,hash=0ouYK2tXro2mNv0xkoB6Cg==")]
        [TestCase("MaybeboardAndAlternateArtCollectorNumber.txt", ExpectedResult = "count=108,hash=zHg9fZWqNKk6wlJmoLBu0w==")]
        [TestCase("ComboPieceRelatedCards.txt", ExpectedResult = "count=115,hash=hthbJgpnr1Ul4CI3x8cB7A==")]
        [TestCase("Foils.txt", ExpectedResult = "count=107,hash=6ihzVsB60rC39ZlEJSTIsA==")]
        [TestCase("SameNameTokens.txt", ExpectedResult = "count=115,hash=hthbJgpnr1Ul4CI3x8cB7A==")]
        public async Task<string> TestRunsEndToEndWithoutErrors(string sampleName)
        {
            using var sample = SamplesHelper.GetSample(sampleName);

            var testLogger = new TestLogger();
            var importer = new Importer(testLogger, TestConfiguration.Configuration);
            var result = await importer.TryImportAsync(new DeckFileInput(sample.Path));
            Assert.IsEmpty(testLogger.ErrorLines);
            Assert.IsEmpty(testLogger.WarningLines);
            Assert.IsTrue(result);

            var outputFile = Path.Combine(TestConfiguration.Configuration.OutputDirectory, Path.ChangeExtension(Path.GetFileName(sample.Path), ".json"));
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
    }
}

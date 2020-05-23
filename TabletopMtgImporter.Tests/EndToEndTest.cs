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
        [TestCase("Archidekt1CardNameFormat.txt")]
        [TestCase("Archidekt1xCardNameCodeCategoryLabel.txt")]
        [TestCase("MaybeboardAndAlternateArtCollectorNumber.txt")]
        [TestCase("ComboPieceRelatedCards.txt")]
        [TestCase("Archidekt1xCardNameCodeCategoryLabelWithFoils.txt")]
        public async Task TestRunsEndToEndWithoutErrors(string sampleName)
        {
            using var sample = SamplesHelper.GetSample(sampleName);

            var testLogger = new TestLogger();
            var importer = new Importer(testLogger, TestConfiguration.Configuration);
            Assert.IsTrue(await importer.TryImportAsync(new DeckFileInput(sample.Path)));
            Assert.IsEmpty(testLogger.ErrorLines);
            Assert.IsEmpty(testLogger.WarningLines);

            var outputFile = Path.Combine(TestConfiguration.Configuration.OutputDirectory, Path.ChangeExtension(Path.GetFileName(sample.Path), ".json"));
            Assert.IsTrue(File.Exists(outputFile));

            var outputText = File.ReadAllText(outputFile);
            Assert.DoesNotThrow(() => JsonConvert.DeserializeObject<TabletopDeckObject>(outputText));
        }
    }
}

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
        [TestCase("Archidekt1CardNameFormat.txt", ExpectedResult = "qVp87K8AISOkZVF1RM5Yiw==")]
        [TestCase("Archidekt1xCardNameCodeCategoryLabel.txt", ExpectedResult = "7gVfD5APapb1UJW44Vu9gQ==")]
        [TestCase("MaybeboardAndAlternateArtCollectorNumber.txt", ExpectedResult = "ldTApaFLOzy1MT1Yu1zj4Q==")]
        [TestCase("ComboPieceRelatedCards.txt", ExpectedResult = "lvnt9ZY3tnYbuhrlf3v0RA==")]
        public async Task<string> TestRunsEndToEndWithoutErrors(string sampleName)
        {
            using var sample = SamplesHelper.GetSample(sampleName);

            var testLogger = new TestLogger();
            var importer = new Importer(testLogger, TestConfiguration.Configuration);
            Assert.IsTrue(await importer.TryImportAsync(new DeckFileInput(sample.Path)));
            Assert.IsEmpty(testLogger.ErrorLines);
            Assert.IsEmpty(testLogger.WarningLines);

            var outputFile = Path.Combine(TestConfiguration.Configuration.OutputDirectory, Path.ChangeExtension(Path.GetFileName(sample.Path), ".json"));
            Assert.IsTrue(File.Exists(outputFile));

            using var md5 = MD5.Create();
            var hash = Convert.ToBase64String(md5.ComputeHash(File.ReadAllBytes(outputFile)));
            return hash;
        }
    }
}

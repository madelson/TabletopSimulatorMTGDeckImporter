using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter.Tests
{
    public class EndToEndTest
    {
        [TestCase("Archidekt1CardNameFormat.txt")]
        [TestCase("Archidekt1xCardNameCodeCategoryLabel.txt")]
        public async Task TestRunsEndToEndWithoutErrors(string sampleName)
        {
            using var sample = SamplesHelper.GetSample(sampleName);

            var testLogger = new TestLogger();
            var importer = new Importer(testLogger, TestConfiguration.Configuration);
            Assert.IsTrue(await importer.TryImportAsync(new DeckFileInput(sample.Path)));
            Assert.IsEmpty(testLogger.ErrorLines);
            Assert.IsEmpty(testLogger.WarningLines);
        }
    }
}

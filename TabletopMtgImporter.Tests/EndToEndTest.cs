using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabletopMtgImporter.Console;

namespace TabletopMtgImporter.Tests
{
    public class EndToEndTest
    {
        [TestCase("Archidekt Plain Export Format.txt")]
        public async Task TestRunsEndToEndWithoutErrors(string sampleName)
        {
            using var sample = SamplesHelper.GetSample(sampleName);

            Assert.AreEqual(0, await Program.Main(new[] { sample.Path }));
        }
    }
}

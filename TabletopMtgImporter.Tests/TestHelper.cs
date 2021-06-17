using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter.Tests
{
    [SetUpFixture]
    internal class TestHelper
    {
        public static string OutputDirectory { get; private set; } = default!;

        public static ISaver CreateSaver(ILogger logger) => new DiskSaver(logger) { OutputDirectory = OutputDirectory };

        [OneTimeSetUp]
        public void OneTimeSetUp() 
        {
            OutputDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (Directory.Exists(OutputDirectory))
            {
                Directory.Delete(OutputDirectory, recursive: true);
            }
        }
    }
}

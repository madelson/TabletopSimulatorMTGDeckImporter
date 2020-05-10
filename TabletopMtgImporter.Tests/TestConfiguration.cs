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
    internal class TestConfiguration
    {
        private string _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        public static Configuration Configuration { get; set; } = default!;

        [OneTimeSetUp]
        public void OneTimeSetUp() 
        {
            Configuration = new Configuration { OutputDirectory = this._tempDirectory };
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (Directory.Exists(this._tempDirectory))
            {
                Directory.Delete(this._tempDirectory, recursive: true);
            }
        }
    }
}

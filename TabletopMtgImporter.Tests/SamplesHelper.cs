using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter.Tests
{
    internal static class SamplesHelper
    {
        public static SampleFile GetSample(string name)
        {
            using var resourceStream = typeof(SamplesHelper).Assembly.GetManifestResourceStream($"TabletopMtgImporter.Tests.Samples.{name}")
                ?? throw new FileNotFoundException(name);
            var tempFileName = Path.GetTempFileName();
            using var tempFileStream = File.OpenWrite(tempFileName);
            resourceStream.CopyTo(tempFileStream);
            return new SampleFile(tempFileName);
        }

        public class SampleFile : IDisposable
        {
            public SampleFile(string path)
            {
                this.Path = path;
            }

            public string Path { get; }

            public void Dispose() => File.Delete(this.Path);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter
{
    public sealed class DiskSaver : ISaver
    {
        private readonly ILogger _logger;

        public DiskSaver(ILogger logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string OutputDirectory { get; set; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Tabletop Simulator", "Saves", "Saved Objects");

        public Task SaveAsync(string name, string contents)
        {
            Directory.CreateDirectory(this.OutputDirectory); // ensure created
            var outputPath = Path.Combine(this.OutputDirectory, name);
            File.WriteAllText(outputPath, contents);
            this._logger.Info("Wrote output to " + outputPath);
            return Task.CompletedTask;
        }
    }
}

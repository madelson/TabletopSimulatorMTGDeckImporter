using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter.Tests
{
    internal class TestLogger : ILogger
    {
        public List<string> DebugLines { get; } = new List<string>();
        public List<string> InfoLines { get; } = new List<string>();
        public List<string> ErrorLines { get; } = new List<string>();
        public List<string> WarningLines { get; } = new List<string>();

        public void Debug(string message) => this.DebugLines.Add(message);
        public void Error(string message) => this.ErrorLines.Add(message);
        public void Info(string message) => this.InfoLines.Add(message);
        public void Warning(string message) => this.WarningLines.Add(message);
    }
}

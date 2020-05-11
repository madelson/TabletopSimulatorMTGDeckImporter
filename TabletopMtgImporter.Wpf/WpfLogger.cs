using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter.Wpf
{
    internal class WpfLogger : ILogger
    {
        private static readonly object DebugLogLock = new object();

        private readonly Action<string> _writeToUI;

        private volatile int _warningCount, _errorCount;

        public WpfLogger(Action<string> writeToUI)
        {
            this._writeToUI = writeToUI;
        }

        public int WarningCount => this._warningCount;
        public int ErrorCount => this._errorCount;

        public void Debug(string message)
        {
            lock (DebugLogLock)
            {
                var path = Path.Combine(Path.GetTempPath(), $"{typeof(WpfLogger).Assembly.GetName().Name}.log");
                try
                {
                    var info = new FileInfo(path);
                    if (info.Exists && info.Length > 100000)
                    {
                        File.Delete(path);
                    }

                    File.AppendAllText(path, $"{DateTime.Now}: {message}{Environment.NewLine}");
                }
                catch (Exception ex)
                {
                    this.Info($"Failed to write DEBUG output to {path} ({ex.GetType()} {ex.Message})");
                }
            }
        }

        public void Error(string message)
        {
            ++this._errorCount;
            this._writeToUI($"ERROR: {message}");
        }

        public void Info(string message) => this._writeToUI(message);

        public void Warning(string message)
        {
            ++this._warningCount;
            this._writeToUI($"WARNING: {message}");
        }
    }
}

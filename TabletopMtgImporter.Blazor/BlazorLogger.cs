using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TabletopMtgImporter.Blazor
{
    public class BlazorLogger : ILogger
    {
        private readonly Action<Entry> _log;

        public BlazorLogger(Action<Entry> log)
        {
            this._log = log;
        }

        public void Debug(string message) => Console.WriteLine(message);

        public void Error(string message) => this.LogEntry(EntryType.Error, message);

        public void Info(string message) => this.LogEntry(EntryType.Info, message);

        public void Warning(string message) => this.LogEntry(EntryType.Warning, message);

        private void LogEntry(EntryType type, string message) => this._log(new(type, message));

        public record Entry(EntryType Type, string Message);

        public enum EntryType { Info, Warning, Error }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter.Console
{
    using Console = System.Console;

    internal class ConsoleLogger : ILogger
    {
        public void Debug(string message) => Console.WriteLine(message);
        public void Info(string message) => Console.WriteLine(message);
        public void Error(string message) => Console.Error.WriteLine(message);
        public void Warning(string message) => Console.Error.WriteLine(message);
    }
}

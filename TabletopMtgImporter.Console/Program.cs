using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace TabletopMtgImporter.Console
{
    using Console = System.Console;

    internal static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            if (Debugger.IsAttached && args.Length == 0)
            {
                args = new[] { @"C:\Users\mikea_000\Downloads\Sidar and Ikra.txt" };
            }

            if (args.Contains("--debug"))
            {
                args = args.Where(a => a != "--debug").ToArray();
                Debugger.Launch();
            }

            bool useUwcCards;
            if (args.Contains("--uwc"))
            {
                useUwcCards = true;
                args = args.Where(a => a != "--uwc").ToArray();
            }
            else { useUwcCards = false; }

            if (args.Length != 1)
            {
                Console.Error.WriteLine($"Usage {typeof(Program).Assembly.GetName().Name} <deckFile>");
                return 1;
            }

            var deckFile = args[0];
            if (!File.Exists(deckFile))
            {
                Console.Error.WriteLine($"File '{deckFile}' does not exist");
                return 2;
            }

            var logger = new ConsoleLogger();
            return await new Importer(logger, new DiskCache(), new DiskSaver(logger)).TryImportAsync(new DeckFileInput(deckFile, useUwcCards)) ? 0 : 3;
        }
    }
}

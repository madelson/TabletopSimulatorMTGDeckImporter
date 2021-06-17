using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TabletopMtgImporter.Blazor
{
    public static class Application
    {
        private static readonly Regex InvalidFileNameCharRegex = new Regex(
            $"[{string.Join(string.Empty, Path.GetInvalidFileNameChars().Select(ch => @$"\u{(int)ch:x4}"))}]"
        );

        public static string GetDeckName(string deckText)
        {
            var input = new StringDeckInput(deckText) { Name = string.Empty };
            using var reader = input.OpenReader();

            int commanderCount;
            if (!DeckParser.TryParse(reader, out var cards, out _)
                || (commanderCount = cards.Count(c => c.IsCommander)) == 0) 
            {
                return "New deck";
            }

            return string.Join(
                " and ", 
                cards.Where(c => c.IsCommander)
                    .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(c => CleanCardName(c.Name))
            );

            string CleanCardName(string name)
            {
                var slashIndex = name.IndexOf('/');
                name = slashIndex >= 0 ? name.Substring(0, slashIndex) : name;

                // for partners, strip any moniker for brevity
                if (commanderCount > 1)
                {
                    var commaIndex = name.IndexOf(',');
                    name = commaIndex >= 0 ? name.Substring(0, commaIndex) : name;
                }

                return InvalidFileNameCharRegex.Replace(name, string.Empty);
            }
        }

        public static Task<bool> ImportAsync(
            ILogger logger, 
            IJSRuntime jsRuntime,
            string deckName,
            string deckText)
        {
            var importer = new Importer(logger, new LocalStorageCache(jsRuntime), new DownloadSaver(jsRuntime, logger));
            return importer.TryImportAsync(new StringDeckInput(deckText) { Name = deckName });
        }

        private class StringDeckInput : IDeckInput
        {
            private readonly string _deckText;

            public StringDeckInput(string deckText)
            {
                this._deckText = deckText;
            }

            public string Name { get; set; } = default!;

            public TextReader OpenReader() => new StringReader(this._deckText);
        }
    }
}

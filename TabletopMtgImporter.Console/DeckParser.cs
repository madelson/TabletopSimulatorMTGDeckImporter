using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TabletopMtgImporter
{
    internal static class DeckParser
    {
        public static bool TryParse(TextReader reader, out List<string> cards, out List<string> errorLines)
        {
            cards = new List<string>();
            errorLines = new List<string>();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var match = Regex.Match(line, @"^(?<count>\d+)\s+(?<name>.+?)\s*$");
                    if (!match.Success || !int.TryParse(match.Groups["count"].Value, out var count))
                    {
                        errorLines.Add(line);
                    }
                    else
                    {
                        for (var i = 0; i < count; ++i)
                        {
                            cards.Add(match.Groups["name"].Value);
                        }
                    }
                }
            }

            return errorLines.Count == 0;
        }
    }
}
